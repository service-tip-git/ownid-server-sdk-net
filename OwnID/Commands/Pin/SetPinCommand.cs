using System;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Services;

namespace OwnID.Commands.Pin
{
    public class SetPinCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;

        public SetPinCommand(ICacheItemRepository cacheItemRepository)
        {
            _cacheItemRepository = cacheItemRepository;
        }

        public async Task<CacheItem> ExecuteAsync(string context)
        {
            var random = new Random();
            var pin = random.Next(0, 9999).ToString("D4");

            return await _cacheItemRepository.UpdateAsync(context, item =>
            {
                item.ConcurrentId = Guid.NewGuid().ToString();
                item.SecurityCode = pin;
                item.Status = CacheItemStatus.WaitingForApproval;
            });
        }
    }
}