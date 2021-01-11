using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Cryptography;
using OwnID.Extensibility.Cache;
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
    public class UpgradeToPasscodeTransitionHandler : BaseTransitionHandler<TransitionInput<JwtContainer>>
    {
        private readonly SavePartialConnectionCommand _savePartialConnectionCommand;
        private readonly IJwtService _jwtService;
        private readonly ICookieService _cookieService;

        public UpgradeToPasscodeTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, SavePartialConnectionCommand savePartialConnectionCommand, IJwtService jwtService,
            ICookieService cookieService, bool validateSecurityTokens = true) : base(StepType.UpgradeToPasscode,
            jwtComposer, stopFlowCommand, urlProvider, validateSecurityTokens)
        {
            _savePartialConnectionCommand = savePartialConnectionCommand;
            _jwtService = jwtService;
            _cookieService = cookieService;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType,
                new CallAction(UrlProvider.GetSwitchAuthTypeUrl(context, ConnectionAuthType.Passcode)));
        }

        protected override void Validate(TransitionInput<JwtContainer> input, CacheItem relatedItem)
        {
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<JwtContainer> input,
            CacheItem relatedItem)
        {
            var userData = _jwtService.GetDataFromJwt<UserIdentitiesData>(input.Data.Jwt).Data;

            await _savePartialConnectionCommand.ExecuteAsync(userData, relatedItem);

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