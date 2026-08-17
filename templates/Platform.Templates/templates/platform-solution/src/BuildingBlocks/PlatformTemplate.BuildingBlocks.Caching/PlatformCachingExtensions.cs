using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace PlatformTemplate.BuildingBlocks.Caching;

public static class PlatformCachingExtensions
{
    public static IServiceCollection AddPlatformRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(RedisOptions.SectionName);
        var connectionString = section[nameof(RedisOptions.ConnectionString)]
            ?? throw new InvalidOperationException("Redis:ConnectionString не настроен.");

        services.AddOptions<RedisOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Redis:ConnectionString обязателен.")
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
