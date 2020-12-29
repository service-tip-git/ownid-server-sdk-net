using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Providers;
using OwnID.Extensibility.Services;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.TransitionHandlers
{
    public class CheckUserExistenceBaseTransitionHandler : BaseTransitionHandler<TransitionInput<UserIdentification>>
    {
        private readonly CheckUserExistenceCommand _checkUserExistenceCommand;
        private readonly ILocalizationService _localizationService;

        public CheckUserExistenceBaseTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, CheckUserExistenceCommand checkUserExistenceCommand,
            ILocalizationService localizationService) : base(StepType.CheckUserExistence, jwtComposer, stopFlowCommand,
            urlProvider)
        {
            _checkUserExistenceCommand = checkUserExistenceCommand;
            _localizationService = localizationService;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType, new CallAction(UrlProvider.GetUserExistenceUrl(context)));
        }

        protected override void Validate(TransitionInput<UserIdentification> input, CacheItem relatedItem)
        {
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<UserIdentification> input,
            CacheItem relatedItem)
        {
            var result = await _checkUserExistenceCommand.ExecuteAsync(input.Data);

            if (result)
                throw new OwnIdException(ErrorType.UserAlreadyExists);

            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Locale = input.CultureInfo?.Name,
                Behavior = GetNextBehaviorFunc(input, relatedItem)
            };

            var jwt = JwtComposer.GenerateBaseStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}