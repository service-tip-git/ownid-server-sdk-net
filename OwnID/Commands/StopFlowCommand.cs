using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Services;

namespace OwnID.Commands
{
    public class StopFlowCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;

        public StopFlowCommand(ICacheItemRepository cacheItemRepository)
        {
            _cacheItemRepository = cacheItemRepository;
        }

        public async Task ExecuteAsync(string context, string errorMessage)
        {
            await _cacheItemRepository.UpdateAsync(context, cacheItem =>
            {
                cacheItem.Status = CacheItemStatus.Finished;
                cacheItem.Error = errorMessage;
            });
        }
    }
}