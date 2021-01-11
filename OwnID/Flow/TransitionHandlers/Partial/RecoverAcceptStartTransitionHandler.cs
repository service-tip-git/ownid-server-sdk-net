using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Commands.Fido2;
using OwnID.Commands.Recovery;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;

namespace OwnID.Flow.TransitionHandlers.Partial
{
    public class RecoverAcceptStartTransitionHandler : AcceptStartTransitionHandler
    {
        private readonly RecoverAccountCommand _recoverAccountCommand;

        public RecoverAcceptStartTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, IOwnIdCoreConfiguration coreConfiguration,
            TrySwitchToFido2FlowCommand trySwitchToFido2FlowCommand, RecoverAccountCommand recoverAccountCommand,
            SetNewEncryptionTokenCommand setNewEncryptionTokenCommand, IIdentitiesProvider identitiesProvider,
            VerifyFido2CredentialIdCommand verifyFido2CredentialIdCommand) : base(jwtComposer, stopFlowCommand,
            urlProvider, coreConfiguration, trySwitchToFido2FlowCommand, setNewEncryptionTokenCommand,
            identitiesProvider, verifyFido2CredentialIdCommand)
        {
            _recoverAccountCommand = recoverAccountCommand;
        }

        protected override void Validate(TransitionInput<AcceptStartRequest> input, CacheItem relatedItem)
        {
            base.Validate(input, relatedItem);

            if (!relatedItem.IsValidForRecover)
                throw new CommandValidationException(
                    "Cache item should be not Finished with Recover challenge type. " +
                    $"Actual Status={relatedItem.Status.ToString()} ChallengeType={relatedItem.ChallengeType}");
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<AcceptStartRequest> input,
            CacheItem relatedItem)
        {
            // TODO: rework preventing fido2 to use recovery link second time
            if (!input.Data.SupportsFido2 || string.IsNullOrEmpty(input.Data.ExtAuthPayload) || string.IsNullOrEmpty(relatedItem.DID))
                relatedItem = await _recoverAccountCommand.ExecuteAsync(relatedItem);
            
            return await base.ExecuteInternalAsync(input, relatedItem);
        }
    }
}