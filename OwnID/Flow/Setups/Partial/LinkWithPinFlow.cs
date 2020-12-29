using System;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class LinkWithPinFlow : BaseFlow
    {
        public LinkWithPinFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.LinkWithPin)
        {
            // 1.Starting 2.PinApprovalStatus 3.AcceptStart
            AddStartingTransitionsWithPin(StepType.Link);

            // 4. Link
            AddHandler<LinkBaseTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}