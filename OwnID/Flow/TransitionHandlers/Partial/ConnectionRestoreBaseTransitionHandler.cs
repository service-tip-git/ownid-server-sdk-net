using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.TransitionHandlers.Partial
{
    public class ConnectionRestoreBaseTransitionHandler : BaseTransitionHandler<TransitionInput>
    {
        private readonly InternalConnectionRecoveryCommand _internalConnectionRecoveryCommand;
        
        public override StepType StepType => StepType.InternalConnectionRecovery;


        public ConnectionRestoreBaseTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, InternalConnectionRecoveryCommand internalConnectionRecoveryCommand) : base(
            jwtComposer, stopFlowCommand, urlProvider)
        {
            _internalConnectionRecoveryCommand = internalConnectionRecoveryCommand;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType, new CallAction(UrlProvider.GetConnectionRecoveryUrl(context)));
        }

        protected override void Validate(TransitionInput input, CacheItem relatedItem)
        {
            if (string.IsNullOrEmpty(relatedItem.RecoveryToken))
                throw new CommandValidationException("No Recovery Token was found");
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput input,
            CacheItem relatedItem)
        {
            var result = await _internalConnectionRecoveryCommand.ExecuteAsync(relatedItem.RequestToken);

            var composeInfo = new BaseJwtComposeInfo(input)
            {
                Behavior = GetNextBehaviorFunc(input, relatedItem),
                EncKey = relatedItem.EncKey,
                EncVector = relatedItem.EncVector
            };

            var jwt = JwtComposer.GenerateRecoveryDataJwt(composeInfo, result);
            return new JwtContainer(jwt);
        }
    }
}