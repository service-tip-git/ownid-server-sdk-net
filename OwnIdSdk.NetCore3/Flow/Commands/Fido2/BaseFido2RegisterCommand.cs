using System;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
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
        private readonly IFido2 _fido2;
        private readonly IJwtComposer _jwtComposer;
        private readonly IFlowController _flowController;
        private readonly IOwnIdCoreConfiguration _configuration;
        protected string NewUserId { get; } = Guid.NewGuid().ToString("N");

        protected ICacheItemService CacheItemService { get; }

        protected BaseFido2RegisterCommand(
            IFido2 fido2,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController,
            IOwnIdCoreConfiguration configuration
        )
        {
            _fido2 = fido2;
            CacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _configuration = configuration;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!(input is CommandInput<string>))
                throw new InternalLogicException($"Incorrect input type for {GetType().Name}");
        }


        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var requestInput = input as CommandInput<string>;
            var request = OwnIdSerializer.Deserialize<Fido2RegisterRequest>(requestInput.Data);

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

            var result = await _fido2.MakeNewCredentialAsync(
                fido2Response,
                options,
                async args => await Task.FromResult(true));

            var publicKey = Base64Url.Encode(result.Result.PublicKey);

            await ProcessFido2RegisterResponseAsync(relatedItem, publicKey, result.Result.Counter,
                Base64Url.Encode(result.Result.CredentialId));

            return await GetResultAsync(input, relatedItem, currentStepType);
        }

        protected virtual async Task ProcessFido2RegisterResponseAsync(CacheItem relatedItem, string publicKey,
            uint signatureCounter, string credentialId)
        {
            await CacheItemService.SetFido2DataAsync(
                relatedItem.Context,
                publicKey,
                signatureCounter,
                credentialId
            );
        }

        protected virtual Task<ICommandResult> GetResultAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var jwt = _jwtComposer.GenerateBaseStep(
                relatedItem.Context,
                input.ClientDate, _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                NewUserId, input.CultureInfo?.Name, true);

            return Task.FromResult((ICommandResult) new JwtContainer(jwt));
        }
    }
}