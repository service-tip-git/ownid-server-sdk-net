using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
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


        public Fido2LoginCommand(
            IFido2 fido2,
            IUserHandlerAdapter userHandlerAdapter,
            ICacheItemService cacheItemService,
            IJwtComposer jwtComposer,
            IFlowController flowController)
        {
            _fido2 = fido2;
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
        }


        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<Fido2LoginRequest> request))
                throw new InternalLogicException($"Incorrect input type for {nameof(Fido2LoginCommand)}");

            var storedFido2Info = await _userHandlerAdapter.FindFido2Info(request.Data.Info.UserId);
            if (storedFido2Info == null)
            {
                throw new InternalLogicException($"Can't find user with fido2 key '{request.Data.Info.UserId}'");
            }

            var options = new AssertionOptions
            {
                Challenge = Encoding.ASCII.GetBytes(relatedItem.Context),
                RpId = "localhost",
            };

            var fidoResponse = new AuthenticatorAssertionRawResponse
            {
                Extensions = new AuthenticationExtensionsClientOutputs(),
                Id = Base64Url.Decode(request.Data.Info.CredentialId),
                RawId = Base64Url.Decode(request.Data.Info.CredentialId),
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = Base64Url.Decode(request.Data.Info.AuthenticatorData),
                    ClientDataJson = Base64Url.Decode(request.Data.Info.ClientDataJSON),
                    Signature = Base64Url.Decode(request.Data.Info.Signature),
                    UserHandle = Base64Url.Decode(request.Data.Info.UserId)
                },
                Type = PublicKeyCredentialType.PublicKey
            };

            var result = await _fido2.MakeAssertionAsync(
                fidoResponse,
                options,
                Base64Url.Decode(storedFido2Info.PublickKey),
                storedFido2Info.SignatureCounter,
                (args) =>
                {
                    var storedCredentialId = Base64Url.Decode(storedFido2Info.CredentialId);
                    var storedCredDescriptor = new PublicKeyCredentialDescriptor(storedCredentialId);
                    var credIdValidationResult = storedCredDescriptor.Id.SequenceEqual(args.CredentialId);

                    return Task.FromResult(credIdValidationResult);
                });

            await _cacheItemService.SetPublicKeyAsync(relatedItem.Context, storedFido2Info.PublickKey,
                result.Counter, request.Data.Info.UserId);

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, request.Data.Info.UserId);

            var jwt = _jwtComposer.GenerateBaseStep(
                relatedItem.Context, 
                _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType), 
                request.Data.Info.UserId, 
                input.CultureInfo?.Name,
                true);

            return new JwtContainer(jwt);
        }
    }
}