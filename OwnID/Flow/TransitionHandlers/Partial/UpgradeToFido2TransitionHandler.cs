using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Commands.Fido2;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Flow.ResultActions;
using OwnID.Flow.TransitionHandlers.Fido2;
using OwnID.Services;

namespace OwnID.Flow.TransitionHandlers.Partial
{
    public class UpgradeToFido2TransitionHandler : Fido2BaseTransitionHandler
    {
        private readonly Fido2UpgradeConnectionCommand _fido2UpgradeConnectionCommand;

        public override StepType StepType => StepType.UpgradeToFido2;

        public UpgradeToFido2TransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, ICookieService cookieService,
            Fido2UpgradeConnectionCommand fido2UpgradeConnectionCommand) : base(jwtComposer, stopFlowCommand,
            urlProvider, cookieService)
        {
            _fido2UpgradeConnectionCommand = fido2UpgradeConnectionCommand;
        }

        public override FrontendBehavior GetOwnReference(string context, ChallengeType challengeType)
        {
            return new(StepType, challengeType,
                new CallAction(UrlProvider.GetSwitchAuthTypeUrl(context, ConnectionAuthType.Fido2)));
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<string> input,
            CacheItem relatedItem)
        {
            relatedItem = await _fido2UpgradeConnectionCommand.ExecuteAsync(input.Data, relatedItem);

            return GenerateResult(input, relatedItem);
        }
    }
}