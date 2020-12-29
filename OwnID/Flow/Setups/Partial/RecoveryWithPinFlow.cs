using System;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class RecoveryWithPinFlow : BaseFlow
    {
        public RecoveryWithPinFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.RecoverWithPin)
        {
            // 1.Starting 2.PinApprovalStatus 3.AcceptStart
            AddStartingTransitionsWithPin<RecoverAcceptStartTransitionHandler>(StepType.Recover);

            // 4. Recover
            AddHandler<RecoveryTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}