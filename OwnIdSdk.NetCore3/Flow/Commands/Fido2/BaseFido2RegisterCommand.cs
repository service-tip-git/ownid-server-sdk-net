using System;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Json;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public abstract class BaseFido2RegisterCommand : BaseFlowCommand
    {
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IFido2 _fido2;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;

        protected BaseFido2RegisterCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration, IJwtService jwtService)
        {
            _fido2 = fido2;
            CacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        protected string NewUserId { get; } = Guid.NewGuid().ToString("N");

        protected ICacheItemService CacheItemService { get; }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            // if (!(input is CommandInput<JwtContainer>))
            //     throw new InternalLogicException($"Incorrect input type for {GetType().Name}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            Fido2RegisterRequest request;
            if (input is CommandInput<JwtContainer> requestJwt)
            {
                request = _jwtService.GetDataFromJwt<Fido2RegisterRequest>(requestJwt.Data.Jwt).Data;
            }
            else
            {
                var requestInput = input as CommandInput<string>;
                request = OwnIdSerializer.Deserialize<Fido2RegisterRequest>(requestInput.Data);
            }

            if (request == null)
                throw new InternalLogicException($"Incorrect Fido2 register request");

            var fido2Response = new AuthenticatorAttestationRawResponse
            {
                Id = Base64Url.Decode(NewUserId),
                RawId = Base64Url.Decode(NewUserId),
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAttestationRawResponse.ResponseData
                {
                    AttestationObject = Base64Url.Decode(request.Info.AttestationObject),
                    ClientDataJson = Base64Url.Decode(request.Info.ClientDataJSON)
                }
            };

            var options = new CredentialCreateOptions
            {
                Challenge = Encoding.ASCII.GetBytes(relatedItem.Context),
                Rp = new PublicKeyCredentialRpEntity(
                    _configuration.Fido2.RelyingPartyId,
                    _configuration.Fido2.RelyingPartyName,
                    null),
                User = new Fido2User
                {
                    DisplayName = _configuration.Fido2.UserDisplayName,
                    Name = _configuration.Fido2.UserName,
                    Id = Base64Url.Decode(NewUserId)
                }
            };

            var result =
                await _fido2.MakeNewCredentialAsync(fido2Response, options, args => Task.FromResult(true));

            var publicKey = Base64Url.Encode(result.Result.PublicKey);

            await ProcessFido2RegisterResponseAsync(relatedItem, publicKey, result.Result.Counter,
                Base64Url.Encode(result.Result.CredentialId));

            return await GetResultAsync(input, relatedItem, currentStepType);
        }

        protected virtual async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            await CacheItemService.SetFido2DataAsync(relatedItem.Context, publicKey, signatureCounter, credentialId);
        }

        protected virtual Task<ICommandResult> GetResultAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name,
                IncludeRequester = true,
                EncToken = relatedItem.EncToken
            };
            
            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, NewUserId);

            return Task.FromResult((ICommandResult) new JwtContainer(jwt));
        }
    }
}