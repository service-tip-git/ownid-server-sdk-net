using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OwnID.Extensibility.Cache;
using OwnID.Extensibility.Configuration;
using OwnID.Extensibility.Json;
using StackExchange.Redis;

namespace OwnID.Redis
{
    public class RedisCacheStore : ICacheStore
    {
        private readonly IDatabase _redisDb;
        private readonly string _keyPrefix;

        public RedisCacheStore(IConfiguration configuration, IOwnIdCoreConfiguration coreConfiguration) : this(
            configuration.GetSection("ownid")?["cache_config"])
        {
            _keyPrefix = coreConfiguration.DID;
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

            var isSuccess = _redisDb.StringSet($"{_keyPrefix}{key}", serializedData, expiration);

            if (!isSuccess)
                throw new Exception($"Can not set element to redis with context {data.Context}");
        }

        public async Task SetAsync(string key, CacheItem data, TimeSpan expiration)
        {
            var serializedData = OwnIdSerializer.Serialize(data);

            var isSuccess = await _redisDb.StringSetAsync($"{_keyPrefix}{key}", serializedData, expiration);
            
            if (!isSuccess)
                throw new Exception($"Can not set element to redis with context {data.Context}");
        }

        public CacheItem Get(string key)
        {
            var item = _redisDb.StringGet($"{_keyPrefix}{key}");

            if (item.IsNullOrEmpty)
                return null;

            return OwnIdSerializer.Deserialize<CacheItem>(item.ToString());
        }

        public async Task<CacheItem> GetAsync(string key)
        {
            var item = await _redisDb.StringGetAsync($"{_keyPrefix}{key}");

            if (item.IsNullOrEmpty)
                return null;

            return OwnIdSerializer.Deserialize<CacheItem>(item.ToString());
        }

        public void Remove(string key)
        {
            _redisDb.KeyDelete($"{_keyPrefix}{key}", CommandFlags.FireAndForget);
        }

        public async Task RemoveAsync(string key)
        {
            await _redisDb.KeyDeleteAsync($"{_keyPrefix}{key}", CommandFlags.FireAndForget);
        }

        public async Task<(long keysCount, long itemsSize)> GetMemoryStatsAsync()
        {
            var result = (await _redisDb.ExecuteAsync("memory", "stats")).ToDictionary();
            return (long.Parse(result["keys.count"].ToString() ?? "0"), long.Parse(result["dataset.bytes"].ToString() ?? "0"));
        }
    }
}