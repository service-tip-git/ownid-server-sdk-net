using System;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Exceptions;
using OwnIdSdk.NetCore3.Extensibility.Flow;
using OwnIdSdk.NetCore3.Flow.Steps;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public abstract class BaseFlowCommand
    {
        public async Task<ICommandResult> ExecuteAsync(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType, bool requiresTokensValidation = true)
        {
            BasicValidation(input, relatedItem);
            
            if(requiresTokensValidation) 
                ValidateCacheItemTokens(relatedItem, input);
            
            Validate(input, relatedItem);

            return await ExecuteInternal(input, relatedItem, currentStepType);
        }

        protected abstract void Validate(ICommandInput input, CacheItem relatedItem);

        protected abstract Task<ICommandResult> ExecuteInternal(ICommandInput input, CacheItem relatedItem,
            StepType currentStepType);

        protected void ValidateCacheItemTokens(CacheItem item, ICommandInput commandInput)
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
            if(input == null)
                throw new ArgumentException($"{nameof(input)} param can not be null");
            
            if(relatedItem == null)
                throw new ArgumentException($"{nameof(relatedItem)} param can not be null");
        }
    }
}