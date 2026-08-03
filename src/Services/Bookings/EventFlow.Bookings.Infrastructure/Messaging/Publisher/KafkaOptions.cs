namespace EventFlow.Bookings.Infrastructure.Messaging.Publisher;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; init; } = string.Empty;

}
