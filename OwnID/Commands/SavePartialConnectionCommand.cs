using System;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Flow.Contracts.Jwt;
using OwnID.Extensions;
using OwnID.Services;

namespace OwnID.Commands
{
    public class SavePartialConnectionCommand
    {
        private readonly ICacheItemRepository _cacheItemRepository;

        public SavePartialConnectionCommand(ICacheItemRepository cacheItemRepository)
        {
            _cacheItemRepository = cacheItemRepository;
        }

        public async Task ExecuteAsync(UserIdentitiesData input, CacheItem relatedItem)
        {
            var recoveryToken = !string.IsNullOrEmpty(input.RecoveryData) ? Guid.NewGuid().ToString("N") : null;

            await _cacheItemRepository.UpdateAsync(relatedItem.Context, item =>
            {
                item.RecoveryToken = recoveryToken;
                item.RecoveryData = input.RecoveryData;
                item.FinishFlow(input.DID, input.PublicKey);
            });
        }
    }
}