using System.Linq;
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
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Fido2
{
    public class Fido2LoginCommand : BaseFlowCommand
    {
        private readonly IFido2 _fido2;
        private readonly IUserHandlerAdapter _userHandlerAdapter;
        private readonly ICacheItemService _cacheItemService;
        private readonly IJwtComposer _jwtComposer;
        private readonly IFlowController _flowController;
        private readonly IOwnIdCoreConfiguration _configuration;

        public Fido2LoginCommand(
            IFido2 fido2,
            IUserHandlerAdapter userHandlerAdapter,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController, IOwnIdCoreConfiguration configuration)
        {
            _fido2 = fido2;
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _configuration = configuration;
        }


        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!(input is CommandInput<string>))
                throw new InternalLogicException($"Incorrect input type for {nameof(Fido2LoginCommand)}");
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var requestInput = input as CommandInput<string>;
            var request = OwnIdSerializer.Deserialize<Fido2LoginRequest>(requestInput.Data);

            request.Info = request.Info;
            var frontendBehavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);

            var storedFido2Info = await _userHandlerAdapter.FindFido2Info(request.Info.CredentialId);
            if (storedFido2Info == null)
            {
                // TODO: add fail finish method + handling
                await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, request.Info.CredentialId,
                    string.Empty);

                var jwt2 = _jwtComposer.GenerateBaseStep(relatedItem.Context, input.ClientDate, frontendBehavior,
                    request.Info.CredentialId, input.CultureInfo?.Name, true);

                return new JwtContainer(jwt2);
            }

            var options = new AssertionOptions
            {
                Challenge = Encoding.ASCII.GetBytes(relatedItem.Context),
                RpId = _configuration.Fido2.RelyingPartyId,
            };
            
            var fidoResponse = new AuthenticatorAssertionRawResponse
            {
                Extensions = new AuthenticationExtensionsClientOutputs(),
                Id = Base64Url.Decode(request.Info.CredentialId),
                RawId = Base64Url.Decode(request.Info.CredentialId),
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = Base64Url.Decode(request.Info.AuthenticatorData),
                    ClientDataJson = Base64Url.Decode(request.Info.ClientDataJSON),
                    Signature = Base64Url.Decode(request.Info.Signature),
                    UserHandle = Base64Url.Decode(storedFido2Info.UserId)
                },
                Type = PublicKeyCredentialType.PublicKey
            };

            var result = await _fido2.MakeAssertionAsync(
                fidoResponse,
                options,
                Base64Url.Decode(storedFido2Info.PublicKey),
                storedFido2Info.SignatureCounter,
                (args) =>
                {
                    var storedCredentialId = Base64Url.Decode(storedFido2Info.CredentialId);
                    var storedCredDescriptor = new PublicKeyCredentialDescriptor(storedCredentialId);
                    var credIdValidationResult = storedCredDescriptor.Id.SequenceEqual(args.CredentialId);

                    return Task.FromResult(credIdValidationResult);
                });

            await _cacheItemService.SetFido2DataAsync(relatedItem.Context,
                storedFido2Info.PublicKey,
                result.Counter,
                request.Info.CredentialId);

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context,
                storedFido2Info.UserId,
                storedFido2Info.PublicKey);

            var jwt = _jwtComposer.GenerateBaseStep(
                relatedItem.Context,
                input.ClientDate, _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                storedFido2Info.UserId, input.CultureInfo?.Name, true);

            return new JwtContainer(jwt);
        }
    }
}