using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;
using OwnIdSdk.NetCore3.Extensibility.Providers;
using OwnIdSdk.NetCore3.Flow.Interfaces;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands.Authorize
{
    public class GetAuthProfileCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IJwtComposer _jwtComposer;

        public GetAuthProfileCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IIdentitiesProvider identitiesProvider)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _identitiesProvider = identitiesProvider;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForAuthorize)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Login or Register challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType)
        {
            var jwt = _jwtComposer.GenerateProfileConfigJwt(
                relatedItem.Context,
                input.ClientDate, _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                _identitiesProvider.GenerateUserId(), input.CultureInfo?.Name, true);

            return Task.FromResult(new JwtContainer(jwt) as ICommandResult);
        }
    }
}