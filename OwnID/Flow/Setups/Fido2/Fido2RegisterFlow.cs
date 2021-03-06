using System;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Fido2;

namespace OwnID.Flow.Setups.Fido2
{
    public class Fido2RegisterFlow : BaseFlow
    {
        public Fido2RegisterFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.Fido2Register)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(StepType.Fido2Authorize);

            // 3. Fido2Authorize (register)
            AddHandler<Fido2RegisterTransitionHandler, TransitionInput<string>>(OnSuccess);
        }

        private FrontendBehavior OnSuccess(TransitionInput<string> _, CacheItem item)
        {
            var challengeType = item.ChallengeType;
            if (item.ChallengeType == ChallengeType.Register && item.InitialChallengeType == ChallengeType.Login)
                challengeType = ChallengeType.LinkOnLogin;
            
            return FrontendBehavior.CreateSuccessFinish(challengeType);
        }
    }
}