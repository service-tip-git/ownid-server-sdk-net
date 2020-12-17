using System;
using System.Threading.Tasks;
using OwnID.Flow.Steps;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Exceptions;
using OwnID.Extensibility.Flow;

namespace OwnID.Flow.Commands
{
    public abstract class BaseFlowCommand
    {
        // TODO stateless as attribute
        public async Task<ICommandResult> ExecuteAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType, bool requiresTokensValidation = true)
        {
            BasicValidation(input, relatedItem);

            if (requiresTokensValidation)
                ValidateCacheItemTokens(relatedItem, input);

            Validate(input, relatedItem);

            return await ExecuteInternalAsync(input, relatedItem, currentStepType);
        }

        protected abstract void Validate(ICommandInput input, CacheItem relatedItem);

        protected abstract Task<ICommandResult> ExecuteInternalAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType);

        private void ValidateCacheItemTokens(CacheItem item, ICommandInput commandInput)
        {
            if (item.RequestToken != commandInput.RequestToken)
                throw new CommandValidationException(
                    $"{nameof(commandInput.RequestToken)} doesn't match. Expected={item.RequestToken} Actual={commandInput.RequestToken}");

            if (item.ResponseToken != commandInput.ResponseToken)
                throw new CommandValidationException(
                    $"{nameof(commandInput.ResponseToken)} doesn't match. Expected={item.ResponseToken} Actual={commandInput.ResponseToken}");
        }

        private static void BasicValidation(ICommandInput input, CacheItem relatedItem)
        {
            if (input == null)
                throw new ArgumentException($"{nameof(input)} param can not be null");

            if (relatedItem == null)
                throw new ArgumentException($"{nameof(relatedItem)} param can not be null");
        }
    }
}