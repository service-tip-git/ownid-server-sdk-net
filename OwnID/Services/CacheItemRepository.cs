using System;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;

namespace OwnID.Services
{
    public class CacheItemRepository : ICacheItemRepository
    {
        private readonly ICacheStore _cacheStore;
        private readonly TimeSpan _itemExpiration;

        public CacheItemRepository(ICacheStore cacheStore, IOwnIdCoreConfiguration coreConfiguration)
        {
            _cacheStore = cacheStore;
            _itemExpiration = TimeSpan.FromMilliseconds(coreConfiguration.CacheExpirationTimeout);
        }

        public async Task<CacheItem> CreateAsync(CacheItem cacheItem, TimeSpan? expiration = null)
        {
            await _cacheStore.SetAsync(cacheItem.Context, cacheItem, expiration ?? _itemExpiration);
            return cacheItem;
        }

        public async Task<CacheItem> GetAsync(string context, bool withErrorOnNull = true)
        {
            var item = await _cacheStore.GetAsync(context);

            if (withErrorOnNull && item == null)
                throw new ArgumentException($"Can not find any item with context '{context}'");

            return item;
        }

        public async Task<CacheItem> UpdateAsync(string context, Action<CacheItem> updateAction,
            TimeSpan? expiration = null)
        {
            var item = await GetAsync(context);
            updateAction(item);
            item.Context = context;

            await _cacheStore.SetAsync(context, item, expiration ?? _itemExpiration);
            return item;
        }

        public Task RemoveAsync(string context)
        {
            return _cacheStore.RemoveAsync(context);
        }
    }
}