using System;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers;
using OwnID.Flow.TransitionHandlers.Partial;

namespace OwnID.Flow.Setups.Partial
{
    public class PartialAuthorizeFlow : BasePartialFlow
    {
        public PartialAuthorizeFlow(IServiceProvider serviceProvider, IOwnIdCoreConfiguration ownIdCoreConfiguration) :
            base(serviceProvider, FlowType.PartialAuthorize, ownIdCoreConfiguration)
        {
            // 1.Starting 2.AcceptStart
            AddStartingTransitions(GetOnStartAcceptBehavior);

            // 3. (optional) ConnectionRestore
            AddHandler<ConnectionRestoreBaseTransitionHandler, TransitionInput>((_, item) =>
                GetOnRecoveryConnectionPassedBehavior(item));

            // 4. (optional) CheckUserExistence
            AddHandler<CheckUserExistenceBaseTransitionHandler, TransitionInput<UserIdentification>>((_, item) =>
                GetOnInstantAuthorizeBehavior(item));

            // 5. InstantAuthorize
            AddHandler<InstantAuthorizeBaseTransitionHandler, TransitionInput<JwtContainer>>((_, item) =>
                FrontendBehavior.CreateSuccessFinish(item.ChallengeType));
        }

        private FrontendBehavior GetOnStartAcceptBehavior(TransitionInput<AcceptStartRequest> input,
            CacheItem cacheItem)
        {
            // Recover connection if there is no such but recovery token is available
            if (!input.Data.AuthType.HasValue && !string.IsNullOrEmpty(cacheItem.RecoveryToken))
                return GetReferenceToExistingStep(StepType.InternalConnectionRecovery, cacheItem.Context,
                    cacheItem.ChallengeType);

            return GetOnRecoveryConnectionPassedBehavior(cacheItem);
        }

        private FrontendBehavior GetOnRecoveryConnectionPassedBehavior(CacheItem cacheItem)
        {
            var authorize = GetOnInstantAuthorizeBehavior(cacheItem);

            if (cacheItem.ChallengeType == ChallengeType.Register && !string.IsNullOrEmpty(cacheItem.EncToken))
            {
                var checkUserExistence = GetReferenceToExistingStep(StepType.CheckUserExistence, cacheItem.Context,
                    cacheItem.ChallengeType);
                checkUserExistence.AlternativeBehavior = authorize;
                return checkUserExistence;
            }

            return authorize;
        }

        private FrontendBehavior GetOnInstantAuthorizeBehavior(CacheItem cacheItem)
        {
            return GetReferenceToExistingStep(StepType.InstantAuthorize, cacheItem.Context,
                cacheItem.ChallengeType);
        }
    }
}