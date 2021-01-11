using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Services;
using OwnID.Services;

namespace OwnID.Commands
{
    public class StopFlowCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;
        private readonly ILocalizationService _localizationService;

        public StopFlowCommand(ICacheItemRepository cacheItemRepository, ILocalizationService localizationService)
        {
            _cacheItemRepository = cacheItemRepository;
            _localizationService = localizationService;
        }

        public async Task ExecuteAsync(string context, string errorMessage)
        {
            // TODO: rework
            var localizedMessage = _localizationService.GetLocalizedString(errorMessage);
            
            await _cacheItemRepository.UpdateAsync(context, cacheItem =>
            {
                cacheItem.Status = CacheItemStatus.Finished;
                cacheItem.Error = localizedMessage;
            });
        }
    }
}