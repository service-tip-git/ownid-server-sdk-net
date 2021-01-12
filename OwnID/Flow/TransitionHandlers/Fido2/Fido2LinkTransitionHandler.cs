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
    public class Fido2LinkTransitionHandler : Fido2BaseTransitionHandler
    {
        private readonly Fido2LinkCommand _fido2LinkCommand;

        public override StepType StepType => StepType.Fido2Authorize;

        public Fido2LinkTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, ICookieService cookieService, Fido2LinkCommand fido2LinkCommand) : base(
            jwtComposer, stopFlowCommand, urlProvider, cookieService)
        {
            _fido2LinkCommand = fido2LinkCommand;
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<string> input,
            CacheItem relatedItem)
        {
            relatedItem = await _fido2LinkCommand.ExecuteAsync(input.Data, relatedItem);
            return GenerateResult(input, relatedItem);
        }
    }
}