using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Services;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Recovery
{
    public class SaveAccountPublicKeyCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IUserHandlerAdapter _userHandlerAdapter;
        private readonly ILocalizationService _localizationService;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public SaveAccountPublicKeyCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IAccountRecoveryHandler recoveryHandler,
            IOwnIdCoreConfiguration coreConfiguration, IUserHandlerAdapter userHandlerAdapter,
            ILocalizationService localizationService)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _recoveryHandler = recoveryHandler;
            _coreConfiguration = coreConfiguration;
            _userHandlerAdapter = userHandlerAdapter;
            _localizationService = localizationService;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForRecover)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Recover challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!(input is CommandInput<JwtContainer> requestJwt))
                throw new InternalLogicException($"Incorrect input type for {nameof(SaveAccountPublicKeyCommand)}");

            var userData = _jwtService.GetDataFromJwt<UserProfileData>(requestJwt.Data.Jwt).Data;

            var userExists = await _userHandlerAdapter.IsUserExists(userData.PublicKey);
            if (userExists)
            {
                await _cacheItemService.FinishFlowWithErrorAsync(relatedItem.Context,
                    _localizationService.GetLocalizedString("Error_PhoneAlreadyConnected"));
            }
            else
            {
                if (!_coreConfiguration.OverwriteFields)
                    userData.Profile = null;

                await _recoveryHandler.OnRecoverAsync(userData.DID, new OwnIdConnection
                {
                    PublicKey = userData.PublicKey,
                    RecoveryToken = relatedItem.RecoveryToken,
                    RecoveryData = userData.RecoveryData
                });

                await _cacheItemService.FinishAuthFlowSessionAsync(input.Context, userData.DID, userData.PublicKey);
            }

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, StepType.Recover),
                Locale = input.CultureInfo?.Name
            };
            var jwt = _jwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}