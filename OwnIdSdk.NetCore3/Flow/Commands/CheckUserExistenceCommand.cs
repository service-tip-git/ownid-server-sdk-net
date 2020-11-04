using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class CheckUserExistenceCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IJwtComposer _jwtComposer;
        private readonly ILocalizationService _localizationService;
        private readonly IUserHandlerAdapter _userHandlerAdapter;

        public CheckUserExistenceCommand(IUserHandlerAdapter userHandlerAdapter, ICacheItemService cacheItemService,
            ILocalizationService localizationService, IJwtComposer jwtComposer, IFlowController flowController,
            IIdentitiesProvider identitiesProvider)
        {
            _userHandlerAdapter = userHandlerAdapter;
            _cacheItemService = cacheItemService;
            _localizationService = localizationService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _identitiesProvider = identitiesProvider;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!(input is CommandInput<UserExistsRequest>))
                throw new InternalLogicException($"Incorrect input type for {nameof(CheckUserExistenceCommand)}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType, bool isStateless)
        {
            var request = input as CommandInput<UserExistsRequest>;
            var result = await Check(request);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name
            };

            if (result && request!.Data.ErrorOnExisting)
                composeInfo.Behavior = new FrontendBehavior
                {
                    Type = StepType.Error,
                    ActionType = ActionType.Finish,
                    ChallengeType = relatedItem.ChallengeType
                };
            else
                // TODO: add webapp routing for !result && input.Data.ErrorOnNoEntry
                composeInfo.Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);

            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, _identitiesProvider.GenerateUserId());
            return new JwtContainer(jwt);
        }

        public async Task<bool> Check(CommandInput<UserExistsRequest> input)
        {
            // Do nothing for recovery flow
            var relatedItem = await _cacheItemService.GetCacheItemByContextAsync(input.Context);
            if (relatedItem.ChallengeType == ChallengeType.Recover)
                return false;

            bool result;

            if (string.IsNullOrWhiteSpace(input.Data.UserIdentifier))
                result = false;
            else if (!input.Data.AuthenticatorType.HasValue)
                result = await _userHandlerAdapter.IsUserExistsAsync(input.Data.UserIdentifier);
            else
                result = input.Data.AuthenticatorType switch
                {
                    ExtAuthenticatorType.Fido2 => await _userHandlerAdapter.IsFido2UserExistsAsync(input.Data
                        .UserIdentifier),
                    _ => false
                };

            if (result && input.Data.ErrorOnExisting)
            {
                var localizedError = _localizationService.GetLocalizedString("Error_PhoneAlreadyConnected");
                await _cacheItemService.FinishFlowWithErrorAsync(input.Context, localizedError);
            }

            if (!result && input.Data.ErrorOnNoEntry)
            {
                var localizedError = _localizationService.GetLocalizedString("Error_UserNotFound");
                await _cacheItemService.FinishFlowWithErrorAsync(input.Context, localizedError);
            }

            return result;
        }
    }
}