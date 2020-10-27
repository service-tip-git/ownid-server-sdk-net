using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class GetNextStepCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly bool _needRequesterInfo;

        public GetNextStepCommand(IJwtComposer jwtComposer, IFlowController flowController,
            bool needRequesterInfo = true)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _needRequesterInfo = needRequesterInfo;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForLink && !relatedItem.IsValidForRecover)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link or Recover challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var step = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = step,
                Locale = input.CultureInfo?.Name,
                IncludeRequester = _needRequesterInfo,
            };

            if (!IsStateless)
            {
                composeInfo.EncToken = relatedItem.EncToken;
                composeInfo.CanBeRecovered = string.IsNullOrEmpty(relatedItem.RecoveryToken);
            }
            
            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, relatedItem.DID);
            return Task.FromResult(new JwtContainer(jwt) as ICommandResult);
        }
    }
}