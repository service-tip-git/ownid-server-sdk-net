using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;
using OwnID.Services;

namespace OwnID.Flow.TransitionHandlers.Partial
{
    public class InstantAuthorizeBaseTransitionHandler : BaseTransitionHandler<TransitionInput<JwtContainer>>
    {
        private readonly ICookieService _cookieService;
        private readonly SavePartialConnectionCommand _savePartialConnectionCommand;

        public InstantAuthorizeBaseTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, SavePartialConnectionCommand savePartialConnectionCommand,
            ICookieService cookieService) : base(StepType.InstantAuthorize, jwtComposer, stopFlowCommand, urlProvider)
        {
            _savePartialConnectionCommand = savePartialConnectionCommand;
            _cookieService = cookieService;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType,
                new CallAction(UrlProvider.GetChallengeUrl(context, challengeType, "/partial")));
        }

        protected override void Validate(TransitionInput<JwtContainer> input, CacheItem relatedItem)
        {
            if (!relatedItem.IsValidForAuthorize)
                throw new CommandValidationException(
                    "Cache item should be not be Finished with PARTIAL Login or Register challenge type. "
                    + $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType.ToString()}");
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<JwtContainer> input,
            CacheItem relatedItem)
        {
            await _savePartialConnectionCommand.ExecuteAsync(input.Data, relatedItem);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = GetNextBehaviorFunc(input, relatedItem),
                Locale = input.CultureInfo?.Name
            };

            var jwt = JwtComposer.GenerateFinalStepJwt(composeInfo);
            return new StateResult(jwt, _cookieService.CreateAuthCookies(relatedItem));
        }
    }
}