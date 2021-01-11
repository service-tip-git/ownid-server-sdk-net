using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Commands.Fido2;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;
using OwnID.Services;

namespace OwnID.Flow.TransitionHandlers.Fido2
{
    public class Fido2RecoveryTransitionHandler : Fido2BaseTransitionHandler
    {
        private readonly Fido2RecoveryCommand _fido2RecoveryCommand;

        public Fido2RecoveryTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, ICookieService cookieService, Fido2RecoveryCommand fido2RecoveryCommand) : base(jwtComposer, stopFlowCommand, urlProvider,
            cookieService)
        {
            _fido2RecoveryCommand = fido2RecoveryCommand;
        }

        protected override void Validate(TransitionInput<string> input, CacheItem relatedItem)
        {
            base.Validate(input, relatedItem);

            if (string.IsNullOrEmpty(relatedItem.DID))
                throw new CommandValidationException("No user DID was found for recovery");
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<string> input,
            CacheItem relatedItem)
        {
            relatedItem = await _fido2RecoveryCommand.ExecuteAsync(input.Data, relatedItem);
            return GenerateResult(input, relatedItem);
        }
    }
}