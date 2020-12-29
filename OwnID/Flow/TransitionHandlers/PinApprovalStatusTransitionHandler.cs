using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Approval;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.TransitionHandlers
{
    public class PinApprovalStatusTransitionHandler : BaseTransitionHandler<TransitionInput>
    {
        private readonly IOwnIdCoreConfiguration _coreConfiguration;

        public PinApprovalStatusTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, IOwnIdCoreConfiguration coreConfiguration) : base(StepType.ApprovePin,
            jwtComposer, stopFlowCommand, urlProvider)
        {
            _coreConfiguration = coreConfiguration;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType,
                new PollingAction(UrlProvider.GetSecurityApprovalStatusUrl(context),
                    _coreConfiguration.PollingInterval));
        }

        protected override void Validate(TransitionInput input, CacheItem relatedItem)
        {
            //TODO: maybe add relatedItem.Status for Approved / Declined / WaitingForApproval ?
        }

        protected override Task<ITransitionResult> ExecuteInternalAsync(TransitionInput input, CacheItem relatedItem)
        {
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name
            };

            string jwt;

            if (relatedItem.Status == CacheItemStatus.Approved)
            {
                composeInfo.Behavior = GetNextBehaviorFunc(input, relatedItem);
                jwt = JwtComposer.GenerateBaseStepJwt(composeInfo);
            }
            else
            {
                composeInfo.Behavior = new FrontendBehavior
                {
                    Type = StepType.Declined,
                    ChallengeType = relatedItem.ChallengeType,
                    ActionType = ActionType.Finish
                };

                jwt = JwtComposer.GenerateFinalStepJwt(composeInfo);
            }

            return Task.FromResult(new GetApprovalStatusResponse(jwt, relatedItem.Status) as ITransitionResult);
        }
    }
}