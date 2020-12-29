using System.Net.Http;
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

namespace OwnID.Flow.TransitionHandlers
{
    public class StartFlowTransitionHandler : BaseTransitionHandler<TransitionInput<StartRequest>>
    {
        protected readonly IIdentitiesProvider IdentitiesProvider;
        protected readonly StartFlowCommand StartFlowCommand;

        public StartFlowTransitionHandler(StartFlowCommand startFlowCommand, StopFlowCommand stopFlowCommand,
            IJwtComposer jwtComposer, IIdentitiesProvider identitiesProvider, IUrlProvider urlProvider) : base(
            StepType.Starting, jwtComposer, stopFlowCommand, urlProvider, false)
        {
            StartFlowCommand = startFlowCommand;
            IdentitiesProvider = identitiesProvider;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType,
                new CallAction(UrlProvider.GetStartFlowUrl(context), HttpMethod.Get.Method));
        }

        protected override void Validate(TransitionInput<StartRequest> input, CacheItem relatedItem)
        {
            var wasContinued = CheckIfContinued(input, relatedItem);
            
            if (wasContinued && relatedItem.RequestToken != input.RequestToken
                             && relatedItem.ResponseToken != input.ResponseToken)
                throw new CommandValidationException(
                    $"Can not continue flow with bypass. Wrong request token actual '{input.RequestToken}' expected '{relatedItem.RequestToken}' or response token actual '{input.ResponseToken}' expected '{relatedItem.ResponseToken}'");
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<StartRequest> input,
            CacheItem relatedItem)
        {
            var wasContinued = CheckIfContinued(input, relatedItem);
            var composeInfo = new BaseJwtComposeInfo
            {
                Context = relatedItem.Context,
                ClientTime = input.ClientDate,
                Behavior = GetNextBehaviorFunc(input, relatedItem),
                Locale = input.CultureInfo?.Name,
                IncludeRequester = true
            };

            var jwt = JwtComposer.GenerateBaseStepJwt(composeInfo, IdentitiesProvider.GenerateUserId());

            if(!wasContinued)
                await StartFlowCommand.ExecuteAsync(new StartFlowCommand.Input
                {
                    Context = input.Context,
                    ResponseJwt = jwt,
                    CredId = input.Data.CredId,
                    EncryptionToken = input.Data.EncryptionToken,
                    RecoveryToken = input.Data.RecoveryToken,
                    RequestToken = input.RequestToken
                });

            return new JwtContainer
            {
                Jwt = jwt
            };
        }
        
        protected virtual bool CheckIfContinued(TransitionInput<StartRequest> input, CacheItem relatedItem)
        {
            return !string.IsNullOrWhiteSpace(input.RequestToken) && !string.IsNullOrWhiteSpace(input.ResponseToken);
        }
    }
}