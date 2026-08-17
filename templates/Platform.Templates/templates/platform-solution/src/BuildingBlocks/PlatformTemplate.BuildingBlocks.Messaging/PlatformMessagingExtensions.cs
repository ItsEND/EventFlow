using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlatformTemplate.BuildingBlocks.Messaging;

public static class PlatformMessagingExtensions
{
    public static IServiceCollection AddPlatformKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(KafkaOptions.SectionName);
        var bootstrapServers = section[nameof(KafkaOptions.BootstrapServers)]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers не настроен.");
        var clientId = section[nameof(KafkaOptions.ClientId)]
            ?? throw new InvalidOperationException("Kafka:ClientId не настроен.");

        services.AddOptions<KafkaOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.BootstrapServers), "Kafka:BootstrapServers обязателен.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Kafka:ClientId обязателен.")
            .ValidateOnStart();

        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = clientId,
                Acks = Acks.All,
                EnableIdempotence = true
            }).Build());
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }
}
