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
    public class Fido2RegisterTransitionHandler : Fido2BaseTransitionHandler
    {
        private readonly Fido2RegisterCommand _fido2RegisterCommand;

        public Fido2RegisterTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, ICookieService cookieService, Fido2RegisterCommand fido2RegisterCommand) : base(
            jwtComposer, stopFlowCommand, urlProvider, cookieService)
        {
            _fido2RegisterCommand = fido2RegisterCommand;
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<string> input,
            CacheItem relatedItem)
        {
            relatedItem = await _fido2RegisterCommand.ExecuteAsync(input.Data, relatedItem);
            return GenerateResult(input, relatedItem);
        }
    }
}