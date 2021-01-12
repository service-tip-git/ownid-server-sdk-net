using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.TransitionHandlers
{
    public class StopFlowTransitionHandler : BaseTransitionHandler<TransitionInput>
    {
        private readonly StopFlowCommand _stopFlowCommand;

        public override StepType StepType => StepType.Stopped;

        public StopFlowTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider) : base(jwtComposer, stopFlowCommand, urlProvider)
        {
            _stopFlowCommand = stopFlowCommand;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType, new CallAction(UrlProvider.GetStopFlowUrl(context)));
        }

        protected override void Validate(TransitionInput input, CacheItem relatedItem)
        {
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput input,
            CacheItem relatedItem)
        {
            // TODO: add text localization
            await _stopFlowCommand.ExecuteAsync(input.Context, "User stopped auth process");
            var composeInfo = new BaseJwtComposeInfo(input)
            {
                Behavior = GetNextBehaviorFunc(input, relatedItem),
            };

            return new JwtContainer(JwtComposer.GenerateFinalStepJwt(composeInfo));
        }
    }
}