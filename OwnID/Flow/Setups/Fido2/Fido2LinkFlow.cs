using System;
using OwnID.Extensibility.Flow;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Fido2;

namespace OwnID.Flow.Setups.Fido2
{
    public class Fido2LinkFlow : BaseFlow
    {
        public Fido2LinkFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.Fido2Link)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(StepType.Fido2Authorize);

            // 3. Fido2Authorize (link)
            AddHandler<Fido2LinkTransitionHandler, TransitionInput<string>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}