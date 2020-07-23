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
            StepType currentStepType)
        {
            Validate();
            // ValidateCacheItemTokens(relatedItem, input);
            return await ExecuteInternal(input, relatedItem, currentStepType);
        }

        protected abstract void Validate();

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
    }
}