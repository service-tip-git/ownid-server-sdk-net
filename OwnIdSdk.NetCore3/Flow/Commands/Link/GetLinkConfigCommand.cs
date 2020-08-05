using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Link
{
    public class GetLinkConfigCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IAccountLinkHandler _linkHandlerAdapter;
        private readonly bool _needRequesterInfo;

        public GetLinkConfigCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IAccountLinkHandler linkHandlerAdapter, bool needRequesterInfo = true)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _linkHandlerAdapter = linkHandlerAdapter;
            _needRequesterInfo = needRequesterInfo;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (_linkHandlerAdapter == null)
                throw new InternalLogicException("Missing Link feature");

            if (!relatedItem.IsValidForLink)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var step = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);

            var jwt = _jwtComposer.GenerateBaseStep(relatedItem.Context, step, relatedItem.DID, input.CultureInfo?.Name,
                _needRequesterInfo);
            return Task.FromResult(new JwtContainer(jwt) as ICommandResult);
        }
    }
}