using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Cryptography;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;
using OwnIdSdk.NetCore3.Services;

namespace OwnIdSdk.NetCore3.Flow.Commands.Recovery
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

            if (!_coreConfiguration.OverwriteFields)
                userData.Profile = null;

            await _recoveryHandler.OnRecoverAsync(userData.DID, new OwnIdConnection
            {
                PublicKey = userData.PublicKey,
                RecoveryToken = userData.RecoveryToken,
                RecoveryData = userData.RecoveryData
            });

            await _cacheItemService.FinishAuthFlowSessionAsync(input.Context, userData.DID, userData.PublicKey);

            var jwt = _jwtComposer.GenerateFinalStepJwt(relatedItem.Context,
                input.ClientDate, _flowController.GetExpectedFrontendBehavior(relatedItem, StepType.Recover),
                input.CultureInfo?.Name);

            return new JwtContainer(jwt);
        }
    }
}