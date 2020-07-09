using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OwnIdSdk.NetCore3.Store;

namespace OwnIdSdk.NetCore3.Web.Store
{
    public class WebCacheStore : ICacheStore        
    {
        private IMemoryCache Cache { get; }

        public WebCacheStore(IMemoryCache cache)
        {
            Cache = cache;
        }
        
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
            return Cache.Get<CacheItem>(key);
        }

        public Task<CacheItem> GetAsync(string key)
        {
            var cachedItem = Cache.Get<CacheItem>(key);
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