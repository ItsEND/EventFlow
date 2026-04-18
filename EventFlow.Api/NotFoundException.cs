namespace EventFlow.Api;

/// <summary>
/// Исключение, выбрасываемое в случае, когда запрашиваемый ресурс не найден.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Наименование ресурса, который не был найден.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Ключ или идентификатор ресурса, который не был найден.
    /// </summary>
    public object? ResourceKey { get; }

    /// <summary>
    /// Инициализирует новый экземпляр исключения <see cref="NotFoundException"/>.
    /// </summary>
    public NotFoundException()
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр исключения <see cref="NotFoundException"/>
    /// с указанным сообщением об ошибке.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    public NotFoundException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр исключения <see cref="NotFoundException"/>
    /// с именем ресурса и его ключом.
    /// </summary>
    /// <param name="resourceName">Имя ресурса.</param>
    /// <param name="resourceKey">Ключ ресурса.</param>
    public NotFoundException(string resourceName, object? resourceKey)
        : base($"{resourceName} с ключом {resourceKey} не найден.")
    {
        ResourceName = resourceName;
        ResourceKey = resourceKey;
    }

    /// <summary>
    /// Инициализирует новый экземпляр исключения <see cref="NotFoundException"/>
    /// с указанным сообщением об ошибке и внутренним исключением.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="innerException">Внутреннее исключение.</param>
    public NotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}