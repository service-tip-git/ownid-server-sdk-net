using System;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class RecoveryWithPinFlow : BasePartialFlow
    {
        public RecoveryWithPinFlow(IServiceProvider serviceProvider, IOwnIdCoreConfiguration coreConfiguration) : base(
            serviceProvider, FlowType.RecoverWithPin, coreConfiguration)
        {
            // 1.Starting 2.PinApprovalStatus 3.AcceptStart
            AddStartingTransitionsWithPin<RecoverAcceptStartTransitionHandler>(StepType.Recover);

            // 4. Recover
            AddHandler<RecoveryTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}