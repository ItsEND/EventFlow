namespace PlatformTemplate.Contracts;

public sealed record IntegrationEventEnvelope<T>(Guid MessageId, DateTimeOffset OccurredAt, string EventType, T Data, string? CorrelationId = null, string? CausationId = null);
