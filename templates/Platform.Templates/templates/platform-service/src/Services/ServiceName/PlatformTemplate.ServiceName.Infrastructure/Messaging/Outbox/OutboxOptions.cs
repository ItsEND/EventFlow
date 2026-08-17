namespace PlatformTemplate.ServiceName.Infrastructure.Messaging.Outbox;

internal sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; init; } = 50;

    public TimeSpan PollInterval { get; init; } = TimeSpan.FromSeconds(5);
}
