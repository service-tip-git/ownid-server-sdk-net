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
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class BaseFido2RegisterCommand : BaseFlowCommand
    {
        private readonly IOwnIdCoreConfiguration _configuration;
        private readonly IFido2 _fido2;
        private readonly IFlowController _flowController;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IEncodingService _encodingService;
        private readonly IJwtComposer _jwtComposer;

        internal BaseFido2RegisterCommand(IFido2 fido2, ICacheItemService cacheItemService, IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration,
            IIdentitiesProvider identitiesProvider, IEncodingService encodingService)
        {
            _fido2 = fido2;
            CacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _configuration = configuration;
            _identitiesProvider = identitiesProvider;
            _encodingService = encodingService;
        }

        protected string NewUserId => _identitiesProvider.GenerateUserId();

        protected ICacheItemService CacheItemService { get; }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!(input is CommandInput<string>))
                throw new InternalLogicException($"Incorrect input type for {GetType().Name}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var request = OwnIdSerializer.Deserialize<Fido2RegisterRequest>((input as CommandInput<string>)!.Data);

            if (string.IsNullOrWhiteSpace(request.AttestationObject))
                throw new InternalLogicException("Incorrect Fido2 register request: AttestationObject is missing");

            if (string.IsNullOrWhiteSpace(request.ClientDataJson))
                throw new InternalLogicException("Incorrect Fido2 register request: ClientDataJson is missing");


            var fido2Response = new AuthenticatorAttestationRawResponse
            {
                Id = _encodingService.Base64UrlDecode(NewUserId),
                RawId = _encodingService.Base64UrlDecode(NewUserId),
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAttestationRawResponse.ResponseData
                {
                    AttestationObject = _encodingService.Base64UrlDecode(request.AttestationObject),
                    ClientDataJson = _encodingService.Base64UrlDecode(request.ClientDataJson)
                }
            };

            var options = new CredentialCreateOptions
            {
                Challenge = _encodingService.ASCIIDecode(relatedItem.Context),
                Rp = new PublicKeyCredentialRpEntity(
                    _configuration.Fido2.RelyingPartyId,
                    _configuration.Fido2.RelyingPartyName,
                    null),
                User = new Fido2User
                {
                    DisplayName = _configuration.Fido2.UserDisplayName,
                    Name = _configuration.Fido2.UserName,
                    Id = _encodingService.Base64UrlDecode(NewUserId)
                }
            };

            var result = await _fido2.MakeNewCredentialAsync(fido2Response, options, args => Task.FromResult(true));

            if (result == null)
                throw new InternalLogicException("Cannot verify fido2 register request");

            var publicKey = _encodingService.Base64UrlEncode(result.Result.PublicKey);

            await ProcessFido2RegisterResponseAsync(relatedItem, publicKey, result.Result.Counter,
                _encodingService.Base64UrlEncode(result.Result.CredentialId));

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
                Locale = input.CultureInfo?.Name
            };

            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, NewUserId);

            return Task.FromResult((ICommandResult) new JwtContainer(jwt));
        }
    }
}