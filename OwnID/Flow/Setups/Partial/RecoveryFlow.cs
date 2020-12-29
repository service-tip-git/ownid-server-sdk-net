using System;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class RecoveryFlow : BaseFlow
    {
        public RecoveryFlow(IServiceProvider serviceProvider) : base(serviceProvider, FlowType.Recover)
        {
            // 1. Starting
            AddHandler<StartFlowTransitionHandler, TransitionInput<StartRequest>>((input, item) =>
                GetReferenceToExistingStep(StepType.AcceptStart, input.Context, item.ChallengeType));

            // 2. AcceptStart
            AddHandler<RecoverAcceptStartTransitionHandler, TransitionInput<AcceptStartRequest>>((input, item) =>
                GetReferenceToExistingStep(StepType.Recover, input.Context, item.ChallengeType));

            // 3. Recover
            AddHandler<RecoveryTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }
    }
}