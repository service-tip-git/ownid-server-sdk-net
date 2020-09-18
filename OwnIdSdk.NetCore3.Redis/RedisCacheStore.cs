using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Cache;
using OwnIdSdk.NetCore3.Extensibility.Json;
using StackExchange.Redis;

namespace OwnIdSdk.NetCore3.Redis
{
    public class RedisCacheStore : ICacheStore
    {
        private readonly IDatabase _redisDb;

        public RedisCacheStore(IConfiguration configuration) : this(
            configuration.GetSection("ownid")?["cache_config"])
        {
        }

        public RedisCacheStore(string configurationString)
        {
            if (string.IsNullOrEmpty(configurationString))
                throw new ArgumentException("No configuration was provided");

            _redisDb = ConnectionMultiplexer.Connect(configurationString).GetDatabase();
        }

        public void Set(string key, CacheItem data, TimeSpan expiration)
        {
            var serializedData = OwnIdSerializer.Serialize(data);

            bool isSuccess;

            if (data.Status == CacheItemStatus.Started)
                isSuccess = _redisDb.StringSet(key, serializedData, expiration);
            else
                isSuccess = _redisDb.StringSet(key, serializedData);

            if (!isSuccess)
                throw new Exception($"Can not set element to redis with context {data.Context}");
        }

        public async Task SetAsync(string key, CacheItem data, TimeSpan expiration)
        {
            var serializedData = OwnIdSerializer.Serialize(data);

            bool isSuccess;

            if (data.Status == CacheItemStatus.Started)
                isSuccess = await _redisDb.StringSetAsync(key, serializedData, expiration);
            else
                isSuccess = await _redisDb.StringSetAsync(key, serializedData);

            if (!isSuccess)
                throw new Exception($"Can not set element to redis with context {data.Context}");
        }

        public CacheItem Get(string key)
        {
            var item = _redisDb.StringGet(key);

            if (item.IsNullOrEmpty)
                return null;

            return OwnIdSerializer.Deserialize<CacheItem>(item.ToString());
        }

        public async Task<CacheItem> GetAsync(string key)
        {
            var item = await _redisDb.StringGetAsync(key);

            if (item.IsNullOrEmpty)
                return null;

            return OwnIdSerializer.Deserialize<CacheItem>(item.ToString());
        }

        public void Remove(string key)
        {
            _redisDb.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public async Task RemoveAsync(string key)
        {
            await _redisDb.KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }
    }
}