using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts.Fido2;
using OwnID.Extensibility.Json;
using OwnID.Extensions;
using OwnID.Flow.Adapters;
using OwnID.Services;

namespace OwnID.Commands.Fido2
{
    public class Fido2LoginCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IFido2 _fido2;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public Fido2LoginCommand(IFido2 fido2, ICacheItemRepository cacheItemRepository,
            IUserHandlerAdapter userHandlerAdapter, IOwnIdCoreConfiguration configuration)
        {
            _fido2 = fido2;
            _cacheItemRepository = cacheItemRepository;
            _userHandlerAdapter = userHandlerAdapter;
            _configuration = configuration;
        }

        public async Task<CacheItem> ExecuteAsync(string fido2Payload, CacheItem relatedItem)
        {
            var request = OwnIdSerializer.Deserialize<Fido2LoginRequest>(fido2Payload);
            
            var storedFido2Info = await _userHandlerAdapter.FindFido2InfoAsync(request.CredentialId);
            if (storedFido2Info == null)
                throw new OwnIdException(ErrorType.UserNotFound);

            var options = new AssertionOptions
            {
                Challenge = Encoding.ASCII.GetBytes(relatedItem.Context),
                RpId = _configuration.Fido2.RelyingPartyId
            };

            var fidoResponse = new AuthenticatorAssertionRawResponse
            {
                Extensions = new AuthenticationExtensionsClientOutputs(),
                Id = Base64Url.Decode(request.CredentialId),
                RawId = Base64Url.Decode(request.CredentialId),
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = Base64Url.Decode(request.AuthenticatorData),
                    ClientDataJson = Base64Url.Decode(request.ClientDataJson),
                    Signature = Base64Url.Decode(request.Signature),
                    UserHandle = Base64Url.Decode(storedFido2Info.UserId)
                },
                Type = PublicKeyCredentialType.PublicKey
            };

            var result = await _fido2.MakeAssertionAsync(
                fidoResponse,
                options,
                Base64Url.Decode(storedFido2Info.PublicKey),
                storedFido2Info.SignatureCounter,
                args =>
                {
                    var storedCredentialId = Base64Url.Decode(storedFido2Info.CredentialId);
                    var storedCredDescriptor = new PublicKeyCredentialDescriptor(storedCredentialId);
                    var credIdValidationResult = storedCredDescriptor.Id.SequenceEqual(args.CredentialId);

                    return Task.FromResult(credIdValidationResult);
                });

            return await _cacheItemRepository.UpdateAsync(relatedItem.Context, item =>
            {
                item.PublicKey = storedFido2Info.PublicKey;
                item.Fido2SignatureCounter = result.Counter;
                item.Fido2CredentialId = request.CredentialId;
                item.FinishFlow(storedFido2Info.UserId, storedFido2Info.PublicKey);
            });
        }
    }
}