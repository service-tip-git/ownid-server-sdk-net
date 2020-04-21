using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Store
{
    public interface ICacheStore
    {
        void Set(string key, CacheItem data);

        Task SetAsync(string key, CacheItem data);

        CacheItem Get(string key);

        Task<CacheItem> GetAsync(string key);
    }
}