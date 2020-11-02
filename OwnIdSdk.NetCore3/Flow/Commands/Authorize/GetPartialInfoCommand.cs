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
    public class GetPartialInfoCommand : BaseFlowCommand
    {
        private readonly IFlowController _flowController;
        private readonly IIdentitiesProvider _identitiesProvider;
        private readonly IJwtComposer _jwtComposer;

        public GetPartialInfoCommand(IJwtComposer jwtComposer, IFlowController flowController,
            IIdentitiesProvider identitiesProvider)
        {
            _jwtComposer = jwtComposer;
            _flowController = flowController;
            _identitiesProvider = identitiesProvider;
        }

        protected override void Validate(ICommandInput input, CacheItem relatedItem)
        {
            if (relatedItem.HasFinalState)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Login or Register challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType, bool isStateless)
        {
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType),
                Locale = input.CultureInfo?.Name,
                IncludeRequester = true
            };

            if (!isStateless)
            {
                composeInfo.EncToken = relatedItem.EncToken;
                composeInfo.CanBeRecovered = !string.IsNullOrEmpty(relatedItem.RecoveryToken);
            }

            var jwt = _jwtComposer.GenerateBaseStepJwt(composeInfo, _identitiesProvider.GenerateUserId());
            return Task.FromResult(new JwtContainer(jwt) as ICommandResult);
        }
    }
}