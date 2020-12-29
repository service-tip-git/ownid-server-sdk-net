using System;
using OwnID.Extensibility.Flow;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Fido2;

namespace OwnID.Flow.Setups.Fido2
{
    public class Fido2LoginFlow : BaseFlow
    {
        public Fido2LoginFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.Fido2Login)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(StepType.Fido2Authorize);

            // 3. Fido2Authorize (login)
            AddHandler<Fido2LoginTransitionHandler, TransitionInput<string>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}