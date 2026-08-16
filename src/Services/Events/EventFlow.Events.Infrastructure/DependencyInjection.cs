using EventFlow.Events.Application.Abstractions.Caching;
using EventFlow.Events.Application.Abstractions.Repositories;
using EventFlow.Events.Infrastructure.Caching;
using EventFlow.Events.Infrastructure.DataAccess;
using EventFlow.Events.Infrastructure.Messaging;
using EventFlow.Events.Infrastructure.Messaging.Inbox;
using EventFlow.Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace EventFlow.Events.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("EventConnection")
            ?? throw new InvalidOperationException("Строка подключения 'EventConnection' не найдена.");

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException(
                "Строка подключения 'Redis:ConnectionString' не найдена.");
        }

        services.AddDbContext<EventDbContext>(options =>
            options.UseNpgsql(dbConnectionString));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConfiguration = ConfigurationOptions.Parse(redisConnectionString);
            redisConfiguration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(redisConfiguration);

        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddOptions<CacheOptions>()
             .Bind(configuration.GetSection(CacheOptions.SectionName))
             .Validate(options => options.EventTtl > TimeSpan.Zero, "EventTtl должен быть больше нуля.")
             .Validate(options => options.TopEventsTtl > TimeSpan.Zero, "TopEventsTtl должен быть больше нуля.");



        services.AddScoped<IEventRepository, EventRepository>();

        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddScoped<BookingConfirmedInboxHandler>();

        services.AddHostedService<KafkaTopicInitializer>();
        services.AddHostedService<BookingConfirmedConsumer>();


        return services;
    }

    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
