using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Flow.Adapters;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Services;

namespace OwnID.Flow.Commands.Link
{
    public class SaveAccountLinkCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IAccountLinkHandler _linkHandler;
        private readonly IUserHandlerAdapter _userHandlerAdapter;
        private readonly ILocalizationService _localizationService;
        private readonly StopFlowCommand _stopFlowCommand;

        public SaveAccountLinkCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IAccountLinkHandler linkHandler,
            IUserHandlerAdapter userHandlerAdapter, ILocalizationService localizationService, StopFlowCommand stopFlowCommand)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _linkHandler = linkHandler;
            _userHandlerAdapter = userHandlerAdapter;
            _localizationService = localizationService;
            _stopFlowCommand = stopFlowCommand;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForLink)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SaveAccountLinkCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserIdentitiesData>(requestJwt.Data.Jwt).Data;

            var userExists = await _userHandlerAdapter.IsUserExistsAsync(userData.PublicKey);
            if (userExists)
            {
                return await _stopFlowCommand.ExecuteAsync(input, ErrorType.UserAlreadyExists,
                    _localizationService.GetLocalizedString("Error_PhoneAlreadyConnected"));
            }

            // preventing data substitution
            userData.DID = relatedItem.DID;

            await _linkHandler.OnLinkAsync(userData.DID, new OwnIdConnection
            {
                PublicKey = userData.PublicKey,
                RecoveryToken = relatedItem.RecoveryToken,
                RecoveryData = userData.RecoveryData
            });

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