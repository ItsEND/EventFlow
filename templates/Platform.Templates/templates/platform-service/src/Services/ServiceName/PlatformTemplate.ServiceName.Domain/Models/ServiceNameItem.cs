namespace PlatformTemplate.ServiceName.Domain.Models;

public sealed class ServiceNameItem
{
    private ServiceNameItem()
    {
    }

    private ServiceNameItem(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static ServiceNameItem Create(string name, DateTimeOffset? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название обязательно.", nameof(name));
        }

        return new ServiceNameItem(Guid.NewGuid(), name.Trim(), createdAt ?? DateTimeOffset.UtcNow);
    }
}
