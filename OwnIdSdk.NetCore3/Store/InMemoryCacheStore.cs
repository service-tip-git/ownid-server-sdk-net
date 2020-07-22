using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Cache;

namespace OwnIdSdk.NetCore3.Store
{
    /// <summary>
    ///     In memory implementation of <see cref="ICacheStore" />
    /// </summary>
    /// <inheritdoc cref="ICacheStore" />
    public class InMemoryCacheStore : ICacheStore
    {
        private readonly ConcurrentDictionary<string, CacheItem> _store;

        public InMemoryCacheStore()
        {
            _store = new ConcurrentDictionary<string, CacheItem>();
        }

        public void Set(string key, CacheItem data, TimeSpan expiration)
        {
            _store.AddOrUpdate(key, data, (s, o) => data);
        }

        public Task SetAsync(string key, CacheItem data, TimeSpan expiration)
        {
            Set(key, data, expiration);
            return Task.CompletedTask;
        }

        public CacheItem Get(string key)
        {
            return !_store.TryGetValue(key, out var item) ? null : item;
        }

        public Task<CacheItem> GetAsync(string key)
        {
            return Task.FromResult(Get(key));
        }

        public void Remove(string key)
        {
            _store.TryRemove(key, out _);
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}