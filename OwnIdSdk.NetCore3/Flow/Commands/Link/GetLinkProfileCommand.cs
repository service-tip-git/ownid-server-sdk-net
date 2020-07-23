using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Flow.Adapters;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Link
{
    public class GetLinkProfileCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IJwtComposer _jwtComposer;
        private readonly IAccountLinkHandlerAdapter _linkHandlerAdapter;
        private readonly bool _needRequesterInfo;

        public GetLinkProfileCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IAccountLinkHandlerAdapter linkHandlerAdapter, bool needRequesterInfo = true)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _linkHandlerAdapter = linkHandlerAdapter;
            _needRequesterInfo = needRequesterInfo;
        }

        protected override void Validate()
        {
            // TODO
        }

        protected override async Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            if (!relatedItem.IsValidForLink)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Link challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");

            var step = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);

            var profile = await _linkHandlerAdapter.GetUserProfileAsync(relatedItem.DID);
            var jwt = _jwtComposer.GenerateProfileWithConfigDataJwt(relatedItem.Context,
                step, relatedItem.DID, profile, input.CultureInfo?.Name, _needRequesterInfo);
            return new JwtContainer(jwt);
        }
    }
}