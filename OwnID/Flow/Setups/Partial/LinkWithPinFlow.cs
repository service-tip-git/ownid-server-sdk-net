using System;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class LinkWithPinFlow : BasePartialFlow
    {
        public LinkWithPinFlow(IServiceProvider serviceProvider, IOwnIdCoreConfiguration ownIdCoreConfiguration) : base(
            serviceProvider, FlowType.LinkWithPin, ownIdCoreConfiguration)
        {
            // 1.Starting 2.PinApprovalStatus 3.AcceptStart
            AddStartingTransitionsWithPin(StepType.Link);

            // 4. Link
            AddHandler<LinkBaseTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}