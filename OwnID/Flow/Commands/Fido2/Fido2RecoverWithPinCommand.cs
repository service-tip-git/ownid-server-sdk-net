using System.Threading.Tasks;
using Fido2NetLib;
using OwnID.Flow.Interfaces;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Providers;

namespace OwnID.Flow.Commands.Fido2
{
    public class Fido2RecoverWithPinCommand : BaseFido2RegisterCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public Fido2RecoverWithPinCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration,
            IIdentitiesProvider identitiesProvider, IEncodingService encodingService,
            IAccountRecoveryHandler recoveryHandler) : base(fido2, cacheItemService, jwtComposer, flowController,
            configuration, identitiesProvider, encodingService)
        {
            _cacheItemService = cacheItemService;
            _recoveryHandler = recoveryHandler;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            var recoverResult = await _recoveryHandler.RecoverAsync(relatedItem.Payload);
            await _recoveryHandler.OnRecoverAsync(recoverResult.DID, new OwnIdConnection
            {
                PublicKey = publicKey,
                Fido2CredentialId = credentialId,
                Fido2SignatureCounter = signatureCounter.ToString()
            });

            relatedItem.DID = recoverResult.DID;

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, recoverResult.DID,
                publicKey);
        }
    }
}