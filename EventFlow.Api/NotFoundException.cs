namespace EventFlow.Api;

public class NotFoundException : Exception
{
    public string? ResourceName { get; }
    public object? ResourceKey { get; }

    public NotFoundException()
    {
    }

    public NotFoundException(string? message) : base(message)
    {
    }

    public NotFoundException(string resourceName, object? resourceKey)
        : base($"{resourceName} с ключом {resourceKey} не найден.")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }

    public NotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}