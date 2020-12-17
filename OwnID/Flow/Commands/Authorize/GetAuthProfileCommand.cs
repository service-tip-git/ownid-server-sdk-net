using System.Threading.Tasks;
using OwnID.Flow.Interfaces;
using OwnID.Flow.Steps;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Providers;

namespace OwnID.Flow.Commands.Authorize
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
            var expectedBehavior = _flowController.GetExpectedFrontendBehavior(relatedItem, currentStepType);
            
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = expectedBehavior,
                Locale = input.CultureInfo?.Name,
                IncludeRequester = true,
                EncToken = relatedItem.EncToken,
                CanBeRecovered = !string.IsNullOrEmpty(relatedItem.RecoveryToken),
                IncludeFido2FallbackBehavior = true
            };
            
            var jwt = _jwtComposer.GenerateProfileConfigJwt(composeInfo, _identitiesProvider.GenerateUserId());
            return Task.FromResult(new JwtContainer(jwt) as ICommandResult);
        }
    }
}