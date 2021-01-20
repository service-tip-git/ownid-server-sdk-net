using System.Threading.Tasks;
using OwnID.Commands;
using OwnID.Commands.Pin;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow.Contracts;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensibility.Flow.Contracts.Start;
using OwnID.Extensibility.Providers;
using OwnID.Flow.Interfaces;

namespace OwnID.Flow.TransitionHandlers
{
    public class StartFlowWithPinTransitionHandler : StartFlowTransitionHandler
    {
        private readonly SetPinCommand _setPinCommand;

        public StartFlowWithPinTransitionHandler(StartFlowCommand startFlowCommand, StopFlowCommand stopFlowCommand,
            IJwtComposer jwtComposer, IIdentitiesProvider identitiesProvider, IUrlProvider urlProvider,
            SetPinCommand setPinCommand) : base(startFlowCommand, stopFlowCommand, jwtComposer, identitiesProvider,
            urlProvider)
        {
            _setPinCommand = setPinCommand;
        }

        protected override void Validate(TransitionInput<StartRequest> input, CacheItem relatedItem)
        {
            base.Validate(input, relatedItem);

            var wasContinued = CheckIfContinued(input, relatedItem);

            if (!wasContinued && relatedItem.Status != CacheItemStatus.Initiated
                              && relatedItem.Status != CacheItemStatus.Started)
                throw new CommandValidationException(
                    $"Wrong status '{relatedItem.Status.ToString()}' for cache item with context '{input.Context}' to set PIN");

            if (!wasContinued && relatedItem.ConcurrentId != null)
                throw new CommandValidationException($"Context '{input.Context}' is already modified with PIN");
        }

        protected override async Task<ITransitionResult> ExecuteInternalAsync(TransitionInput<StartRequest> input,
            CacheItem relatedItem)
        {
            var composeInfo = new BaseJwtComposeInfo(input)
            {
                Behavior = GetNextBehaviorFunc(input, relatedItem),
                IncludeRequester = true
            };

            var wasContinued = CheckIfContinued(input, relatedItem);

            if (!wasContinued)
                relatedItem = await _setPinCommand.ExecuteAsync(input.Context);

            var jwt = JwtComposer.GeneratePinStepJwt(composeInfo, relatedItem.SecurityCode);

            if (!wasContinued)
                await StartFlowCommand.ExecuteAsync(new StartFlowCommand.Input
                {
                    Context = input.Context,
                    ResponseJwt = jwt,
                    CredIdCookieValue = input.Data.CredId,
                    EncryptionTokenCookieValue = input.Data.EncryptionToken,
                    RecoveryTokenCookieValue = input.Data.RecoveryToken,
                    RequestToken = input.RequestToken,
                    ResponseToken = input.ResponseToken
                });

            return new JwtContainer
            {
                Jwt = jwt
            };
        }

        protected override bool CheckIfContinued(TransitionInput<StartRequest> input, CacheItem relatedItem)
        {
            return base.CheckIfContinued(input, relatedItem) && relatedItem.Status == CacheItemStatus.Approved;
        }
    }
}