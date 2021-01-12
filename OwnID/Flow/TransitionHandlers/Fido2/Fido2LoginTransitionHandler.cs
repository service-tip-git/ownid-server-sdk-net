using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Commands.Fido2;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Services;

namespace OwnID.Flow.TransitionHandlers.Fido2
{
    public class Fido2LoginTransitionHandler : Fido2BaseTransitionHandler
    {
        private readonly Fido2LoginCommand _fido2LoginCommand;

        public override StepType StepType => StepType.Fido2Authorize;

        public Fido2LoginTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, ICookieService cookieService, Fido2LoginCommand fido2LoginCommand) : base(
            jwtComposer, stopFlowCommand, urlProvider, cookieService)
        {
            _fido2LoginCommand = fido2LoginCommand;
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<string> input,
            CacheItem relatedItem)
        {
            // TODO: move parsing
            relatedItem = await _fido2LoginCommand.ExecuteAsync(input.Data, relatedItem);
            return GenerateResult(input, relatedItem);
        }
    }
}