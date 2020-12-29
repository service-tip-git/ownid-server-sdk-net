using System;
using System.Threading.Tasks;
using OwnID.Extensibility.Cache;

namespace OwnID.Services
{
    public interface ICacheItemRepository
    {
        Task<CacheItem> CreateAsync(CacheItem cacheItem, TimeSpan? expiration = null);
        
        Task<CacheItem> GetAsync(string context, bool withErrorOnNull = true);

        Task<CacheItem> UpdateAsync(string context, Action<CacheItem> updateAction, TimeSpan? expiration = null);

        Task RemoveAsync(string context);
    }
}