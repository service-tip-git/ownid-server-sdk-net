using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Flow.Adapters;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Flow.Commands.Authorize
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

            if (!string.IsNullOrEmpty(userData.RecoveryData))
                await _cacheItemService.SetRecoveryDataAsync(relatedItem.Context, userData.RecoveryData);

            await _cacheItemService.FinishAuthFlowSessionAsync(relatedItem.Context, userData.DID, userData.PublicKey);
            
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name
            };
            
            var jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}