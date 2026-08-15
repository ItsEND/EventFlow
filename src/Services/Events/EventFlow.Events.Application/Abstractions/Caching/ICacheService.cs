namespace EventFlow.Events.Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default) 
        where T : class;
    Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        where T : class;
    Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
}
