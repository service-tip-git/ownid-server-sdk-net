using System;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class LinkFlow : BaseFlow
    {
        public LinkFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.Link)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(StepType.Link);

            // 3. Link
            AddHandler<LinkBaseTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}