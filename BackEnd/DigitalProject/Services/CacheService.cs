using DigitalProject.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace DigitalProject.Services
{
    public class CacheService : ICacheService
    {
        public const string InstanceName = "DigitalVault:";

        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IDistributedCache cache, IConnectionMultiplexer redis, ILogger<CacheService> logger)
        {
            _cache = cache;
            _redis = redis;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                if (value == null) return default;
                return JsonSerializer.Deserialize<T>(value);
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Redis 無法連線，略過快取讀取：{Key}", key);
                return default;
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Redis 無法連線，略過快取移除：{Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                var db = _redis.GetDatabase();
                var pattern = $"{InstanceName}{prefix}*";

                foreach (var endpoint in _redis.GetEndPoints())
                {
                    var server = _redis.GetServer(endpoint);
                    foreach (var key in server.Keys(db.Database, pattern))
                    {
                        await db.KeyDeleteAsync(key);
                    }
                }
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Redis 無法連線，略過快取清除：{Prefix}", prefix);
            }
        }

        public  async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
                };
                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Redis 無法連線，略過快取寫入：{Key}", key);
            }
        }
    }
}
