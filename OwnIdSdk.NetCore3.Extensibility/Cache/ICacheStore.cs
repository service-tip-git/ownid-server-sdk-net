using System;
using System.Threading.Tasks;

namespace OwnIdSdk.NetCore3.Extensibility.Cache
{
    /// <summary>
    ///     Describes basic methods for implementing <see cref="CacheItem" /> storing mechanism
    /// </summary>
    public interface ICacheStore
    {
        /// <summary>
        ///     Adds or updates <see cref="CacheItem" /> by keys
        /// </summary>
        /// <param name="key">Unique identifier. Context</param>
        /// <param name="data"><see cref="CacheItem"/> to store</param>
        /// <param name="expiration">expiration</param>
        void Set(string key, CacheItem data, TimeSpan expiration);

        /// <summary>
        ///     Adds or updates <see cref="CacheItem" /> by keys
        /// </summary>
        /// <param name="key">Unique identifier. Context</param>
        /// <param name="data"><see cref="CacheItem"/> to store</param>
        /// <param name="expiration">expiration</param>
        /// <remarks>Async version of <see cref="Set"/></remarks>
        Task SetAsync(string key, CacheItem data, TimeSpan expiration);

        /// <summary>
        ///     Get <see cref="CacheItem" /> by <paramref name="key" />
        /// </summary>
        /// <param name="key">Unique identifier. Context</param>
        /// <returns><see cref="CacheItem" /> if was found with such key</returns>
        CacheItem Get(string key);

        /// <summary>
        ///     Gets <see cref="CacheItem" /> by <paramref name="key" />
        /// </summary>
        /// <param name="key">Unique identifier. Context</param>
        /// <returns><see cref="CacheItem" /> if was found with such key</returns>
        /// ///
        /// <remarks>Async version of <see cref="Get" /></remarks>
        Task<CacheItem> GetAsync(string key);

        /// <summary>
        ///     Removes <see cref="CacheItem" /> by <paramref name="key" />
        /// </summary>
        /// <param name="key">Unique identifier. Context</param>
        /// ///
        /// <remarks>Async version of <see cref="Get" /></remarks>
        void Remove(string key);

        Task RemoveAsync(string key);
    }
}