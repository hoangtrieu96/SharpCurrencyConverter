using System.Text.Json;
using StackExchange.Redis;

namespace ConverterService.Data;

public class CacheServiceRedis<T> : ICacheService<T>
{
    private readonly IDatabase _redis;

    public CacheServiceRedis(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<T?> GetAsync(string key)
    {
        var value = await _redis.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync(string key, T value, TimeSpan? expiry = null)
    {
        await _redis.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
    }

    public async Task RemoveAsync(string key)
    {
        await _redis.KeyDeleteAsync(key);
    }
}