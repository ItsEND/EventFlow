namespace PlatformTemplate.ServiceName.Application.Contracts.Events;

public sealed record ServiceNameItemCreated(Guid MessageId, Guid ItemId, string Name, DateTimeOffset CreatedAt);

public static class ServiceNameTopics
{
    public const string ItemCreated = "PlatformTemplate.ServiceName.item-created";
}
