using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Authorize
{
    public class SavePartialProfileCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public SavePartialProfileCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IUserHandlerAdapter userHandlerAdapter)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _userHandlerAdapter = userHandlerAdapter;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForAuthorize)
                throw new CommandValidationException(
                    "Cache item should be not be Finished with PARTIAL Login or Register challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType.ToString()}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SavePartialProfileCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserIdentitiesData>(requestJwt.Data.Jwt).Data;

            if (string.IsNullOrEmpty(userData.PublicKey))
                throw new CommandValidationException("No public key was provided for partial flow");

            if (!string.IsNullOrEmpty(userData.RecoveryToken) && !string.IsNullOrEmpty(userData.RecoveryData))
                await _cacheItemService.SetRecoveryDataAsync(relatedItem.Context, userData.RecoveryToken,
                    userData.RecoveryData);

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, userData.DID, userData.PublicKey);
            var jwt = _jwtComposer.GenerateFinalStepJwt(relatedItem.Context, input.ClientDate,
                _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType), input.CultureInfo?.Name);
            return new JwtContainer(jwt);
        }
    }
}