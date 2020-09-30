using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Recovery
{
    public class RecoverAccountCommand : BaseFlowCommand
    {
        private readonly IAccountRecoveryHandler _accountRecoveryHandler;
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly bool _needRequesterInfo;

        public RecoverAccountCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IAccountRecoveryHandler accountRecoveryHandler = null, bool needRequesterInfo = true)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _accountRecoveryHandler = accountRecoveryHandler;
            _needRequesterInfo = needRequesterInfo;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (_accountRecoveryHandler == null)
                throw new InternalLogicException("Missing Recovery feature");

            if (!relatedItem.IsValidForRecover)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            // Recover access
            var recoverResult = await _accountRecoveryHandler.RecoverAsync(relatedItem.Payload);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name,
                IncludeRequester = _needRequesterInfo,
                EncryptionPassphrase = relatedItem.PasswordlessEncryptionPassphrase
            };

            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, recoverResult.DID);
            return new JwtContainer(jwt);
        }
    }
}