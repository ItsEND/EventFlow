using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace PlatformTemplate.BuildingBlocks.Caching;

internal sealed class RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _database = connection.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue
                ? JsonSerializer.Deserialize<T>(value.ToString())
                : null;
        }
        catch (Exception exception) when (exception is RedisException or JsonException or NotSupportedException)
        {
            logger.LogWarning(exception, "Не удалось прочитать значение Redis по ключу {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiration);
        }
        catch (Exception exception) when (exception is RedisException or JsonException or NotSupportedException)
        {
            logger.LogWarning(exception, "Не удалось записать значение Redis по ключу {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (RedisException exception)
        {
            logger.LogWarning(exception, "Не удалось удалить значение Redis по ключу {CacheKey}", key);
        }
    }
}
