using System;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class LinkFlow : BasePartialFlow
    {
        public LinkFlow(IServiceProvider serviceProvider, IOwnIdCoreConfiguration coreConfiguration) : base(
            serviceProvider, FlowType.Link, coreConfiguration)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(StepType.Link);

            // 3. Link
            AddHandler<LinkBaseTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}