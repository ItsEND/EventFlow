using EventFlow.Events.Application.Abstractions.Repositories;
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

        var cacheConnectionString = configuration.GetConnectionString("Redis")
               ?? throw new InvalidOperationException("Строка подключения 'Redis' не найдена.");

        services.AddDbContext<EventDbContext>(options =>
            options.UseNpgsql(dbConnectionString));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(cacheConnectionString));

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
