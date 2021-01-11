using System;
using OwnID.Extensibility.Flow;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Fido2;

namespace OwnID.Flow.Setups.Fido2
{
    public class Fido2RecoveryFlow : BaseFlow
    {
        public Fido2RecoveryFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.Fido2Recover)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(StepType.Fido2Authorize);

            // 3. Fido2Authorize (recovery)
            AddHandler<Fido2RecoveryTransitionHandler, TransitionInput<string>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}