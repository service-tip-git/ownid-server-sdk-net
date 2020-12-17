using System.Threading.Tasks;
using OwnID.Cryptography;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Services;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Abstractions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;

namespace OwnID.Flow.Commands.Recovery
{
    public class SaveAccountPublicKeyCommand : BaseFlowCommand
    {
        private readonly ICacheItemService _cacheItemService;
        private readonly IOwnIdCoreConfiguration _coreConfiguration;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IJwtService _jwtService;
        private readonly IAccountRecoveryHandler _recoveryHandler;

        public SaveAccountPublicKeyCommand(ICacheItemService cacheItemService, IJwtService jwtService,
            IJwtComposer jwtComposer, IFlowController flowController, IAccountRecoveryHandler recoveryHandler,
            IOwnIdCoreConfiguration coreConfiguration)
        {
            _cacheItemService = cacheItemService;
            _jwtService = jwtService;
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _recoveryHandler = recoveryHandler;
            _coreConfiguration = coreConfiguration;
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

            await _recoveryHandler.RemoveConnectionsAsync(userData.PublicKey);

            if (!_coreConfiguration.OverwriteFields)
                userData.Profile = null;

            await _recoveryHandler.OnRecoverAsync(userData.DID, new OwnIdConnection
            {
                PublicKey = userData.PublicKey,
                RecoveryToken = relatedItem.RecoveryToken,
                RecoveryData = userData.RecoveryData
            });

            await _cacheItemService.FinishAuthFlowSessionAsync(input.Context, userData.DID, userData.PublicKey);

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