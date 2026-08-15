using EventFlow.Events.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace EventFlow.Events.Infrastructure.Caching;

public sealed class RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _redis = connection.GetDatabase();
    public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var value = await _redis.StringGetAsync(cacheKey);
            return value.HasValue 
                ? JsonSerializer.Deserialize<T>(value.ToString()) 
                : null;
        }
        catch(RedisException ex)
        {
            logger.LogWarning(ex, "Не удалось получить значение из Redis по ключу {CacheKey}", cacheKey);
            return null;
        }
        catch(JsonException ex)
        {
            logger.LogWarning(ex, "В Redis находится некорректное значение по ключу {CacheKey}", cacheKey);
            return null;
        }
    }


    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var json = JsonSerializer.Serialize(value);
            await _redis.StringSetAsync(cacheKey, json, expiration);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Не удалось записать значение в Redis по ключу {CacheKey}", cacheKey);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "В Redis находится некорректное значение по ключу {CacheKey}", cacheKey);
            
        }
    }
    public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await _redis.KeyDeleteAsync(cacheKey);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Не удалось удалить значение в Redis по ключу {CacheKey}", cacheKey);

        }
    }
}
