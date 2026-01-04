using StackExchange.Redis;
using System.Text.Json;

// Minimal Redis-backed cache implementing ICacheService
// Uses StackExchange.Redis to persist simple JSON values.
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    // Create service from a multiplexer and get DB instance
    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // Try to get a JSON value and deserialize it to T
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        // Serialize value to JSON and set it with optional expiry
        var json = JsonSerializer.Serialize(value);
        if (expiry.HasValue)
            await _db.StringSetAsync(key, json, expiry.Value);
        else
            await _db.StringSetAsync(key, json);
    }

    public async Task RemoveAsync(string key)
    {
        // Remove a key from Redis
        await _db.KeyDeleteAsync(key);
    }
}
