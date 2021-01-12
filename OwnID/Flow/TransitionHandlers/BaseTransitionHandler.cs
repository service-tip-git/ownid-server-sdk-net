using System;
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
    public abstract class BaseTransitionHandler<TInput> : ITransitionHandler<TInput> where TInput : ITransitionInput
    {
        private readonly StopFlowCommand _stopFlowCommand;
        private readonly bool _validateSecurityTokens;
        protected readonly IJwtComposer JwtComposer;
        protected readonly IUrlProvider UrlProvider;
        
        public abstract StepType StepType { get; }

        protected BaseTransitionHandler(IJwtComposer jwtComposer, StopFlowCommand stopFlowCommand,
            IUrlProvider urlProvider, bool validateSecurityTokens = true)
        {
            JwtComposer = jwtComposer;
            _stopFlowCommand = stopFlowCommand;
            _validateSecurityTokens = validateSecurityTokens;
            UrlProvider = urlProvider;
        }

        public Func<TInput, CacheItem, FrontendBehavior> GetNextBehaviorFunc { get; set; }

        public abstract FrontendBehavior GetOwnReference(string context, ChallengeType challengeType);

        public async Task<ITransitionResult> HandleAsync(ITransitionInput input, CacheItem relatedItem)
        {
            try
            {
                BasicValidation(input, relatedItem);

                if (_validateSecurityTokens)
                    ValidateCacheItemTokens(relatedItem, input);

                var castedInput = (TInput) input;

                Validate(castedInput, relatedItem);
                return await ExecuteInternalAsync(castedInput, relatedItem);
            }
            catch (OwnIdException e)
            {
                if (e.ShouldStopFlow)
                    return await FinishFlowWithError(input, e.ErrorType, e.Message);

                return CreateErrorResponse(input, e.ErrorType);
            }
        }

        protected abstract void Validate(TInput input, CacheItem relatedItem);

        protected abstract Task<ITransitionResult> ExecuteInternalAsync(TInput input, CacheItem relatedItem);

        protected async Task<ITransitionResult> FinishFlowWithError(ITransitionInput input, ErrorType errorType,
            string message)
        {
            await _stopFlowCommand.ExecuteAsync(input.Context, message);
            return CreateErrorResponse(input, errorType);
        }

        protected ITransitionResult CreateErrorResponse(ITransitionInput input, ErrorType errorType)
        {
            var composeInfo = new BaseJwtComposeInfo(input)
            {
                Behavior = FrontendBehavior.CreateError(errorType)
            };

            var jwt = JwtComposer.GenerateFinalStepJwt(composeInfo);
            return new JwtContainer(jwt);
        }

        private void ValidateCacheItemTokens(CacheItem item, ITransitionInput transitionInput)
        {
            if (item.RequestToken != transitionInput.RequestToken)
                throw new CommandValidationException(
                    $"{nameof(transitionInput.RequestToken)} doesn't match. Expected={item.RequestToken} Actual={transitionInput.RequestToken}");

            if (item.ResponseToken != transitionInput.ResponseToken)
                throw new CommandValidationException(
                    $"{nameof(transitionInput.ResponseToken)} doesn't match. Expected={item.ResponseToken} Actual={transitionInput.ResponseToken}");
        }

        private void BasicValidation(ITransitionInput input, CacheItem relatedItem)
        {
            if (input == null)
                throw new ArgumentException($"{nameof(input)} param can not be null");

            if (relatedItem == null)
                throw new ArgumentException($"{nameof(relatedItem)} param can not be null");

            if (!(input is TInput))
                throw new InternalLogicException($"Incorrect input type for {GetType().Name}");

            if (typeof(TInput) == typeof(JwtContainer))
            {
                if (string.IsNullOrWhiteSpace((input as TransitionInput<JwtContainer>)?.Data?.Jwt))
                    throw new CommandValidationException("JWT wasn't provided");
            }

            if (relatedItem.HasFinalState)
                throw new CommandValidationException("Flow is already finished");
        }
    }
}