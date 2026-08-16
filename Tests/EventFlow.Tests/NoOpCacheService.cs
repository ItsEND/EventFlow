using EventFlow.Events.Application.Abstractions.Caching;

namespace EventFlow.Tests;

public sealed class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        where T : class
    {
        return Task.FromResult<T?>(null);
    }
    public Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class
    {
        return Task.CompletedTask;
    }
    public Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
