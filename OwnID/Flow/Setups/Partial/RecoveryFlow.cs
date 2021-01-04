using System;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class RecoveryFlow : BasePartialFlow
    {
        public RecoveryFlow(IServiceProvider serviceProvider, IOwnIdCoreConfiguration coreConfiguration) : base(
            serviceProvider, FlowType.Recover, coreConfiguration)
        {
            // 1. Starting
            AddHandler<StartFlowTransitionHandler, TransitionInput<StartRequest>>((input, item) =>
                GetReferenceToExistingStep(StepType.AcceptStart, input.Context, item.ChallengeType));

            // 2. AcceptStart
            AddHandler<RecoverAcceptStartTransitionHandler, TransitionInput<AcceptStartRequest>>((input, item) =>
            {
                var next = GetReferenceToExistingStep(StepType.Recover, input.Context, item.ChallengeType);
                return TryAddFido2DisclaimerToBehavior(input, item, next);
            });

            // 3. Recover
            AddHandler<RecoveryTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}