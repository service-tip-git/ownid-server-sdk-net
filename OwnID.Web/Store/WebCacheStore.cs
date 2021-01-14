using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OwnID.Extensibility.Cache;

namespace OwnID.Web.Store
{
    public class WebCacheStore : ICacheStore
    {
        public WebCacheStore(IMemoryCache cache)
        {
            Cache = cache;
        }

        private IMemoryCache Cache { get; }

        public void Set(string key, CacheItem data, TimeSpan expiration)
        {
            Cache.Set(key, data, expiration);
        }

        public Task SetAsync(string key, CacheItem data, TimeSpan expiration)
        {
            Cache.Set(key, data, expiration);
            return Task.CompletedTask;
        }

        public CacheItem Get(string key)
        {
            return (CacheItem) Cache.Get<CacheItem>(key).Clone();
        }

        public Task<CacheItem> GetAsync(string key)
        {
            var cachedItem = (CacheItem) Cache.Get<CacheItem>(key).Clone();
            return Task.FromResult(cachedItem);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        public Task RemoveAsync(string key)
        {
            Cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}