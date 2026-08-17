using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlatformTemplate.ServiceName.Application.Abstractions.Persistence;
using PlatformTemplate.ServiceName.Infrastructure.Repositories;
#if (HasDatabase)
using Microsoft.EntityFrameworkCore;
using PlatformTemplate.ServiceName.Infrastructure.DataAccess;
#endif
#if (redis)
using PlatformTemplate.BuildingBlocks.Caching;
#endif
#if (kafka)
using PlatformTemplate.BuildingBlocks.Messaging;
#endif
#if (UseOutbox)
using PlatformTemplate.ServiceName.Application.Abstractions.Messaging;
using PlatformTemplate.ServiceName.Infrastructure.Messaging.Outbox;
#endif

namespace PlatformTemplate.ServiceName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
#if (HasDatabase)
        var connectionString = configuration.GetConnectionString("ServiceNameDatabase")
            ?? throw new InvalidOperationException("Строка подключения 'ServiceNameDatabase' не настроена.");

#if (PostgreSql)
        services.AddDbContext<ServiceNameDbContext>(options => options.UseNpgsql(connectionString));
#elseif (SqlServer)
        services.AddDbContext<ServiceNameDbContext>(options => options.UseSqlServer(connectionString));
#endif

        services.AddScoped<IServiceNameItemRepository, EfServiceNameItemRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
#else
        services.AddSingleton<IServiceNameItemRepository, InMemoryServiceNameItemRepository>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
#endif

#if (redis)
        services.AddPlatformRedis(configuration);
#endif
#if (kafka)
        services.AddPlatformKafka(configuration);
#endif
#if (UseOutbox)
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .Validate(options => options.BatchSize > 0, "Outbox:BatchSize должен быть больше нуля.")
            .Validate(options => options.PollInterval > TimeSpan.Zero, "Outbox:PollInterval должен быть больше нуля.")
            .ValidateOnStart();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddHostedService<OutboxPublisherBackgroundService>();
#endif

        return services;
    }
}
