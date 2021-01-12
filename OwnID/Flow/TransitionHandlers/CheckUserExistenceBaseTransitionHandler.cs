using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;

namespace OwnID.Flow.TransitionHandlers
{
    public class CheckUserExistenceBaseTransitionHandler : BaseTransitionHandler<TransitionInput<UserIdentification>>
    {
        private readonly CheckUserExistenceCommand _checkUserExistenceCommand;

        public override StepType StepType => StepType.CheckUserExistence;

        public CheckUserExistenceBaseTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, CheckUserExistenceCommand checkUserExistenceCommand) : base(jwtComposer,
            stopFlowCommand, urlProvider)
        {
            _checkUserExistenceCommand = checkUserExistenceCommand;
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

            var composeInfo = new BaseJwtComposeInfo(input)
            {
                Behavior = GetNextBehaviorFunc(input, relatedItem)
            };

            var jwt = JwtComposer.GenerateBaseStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }
    }
}