namespace EventFlow.Application.Exceptions;

/// <summary>
/// Единое исключение слоя Application для передачи ошибок во внешние слои.
/// </summary>
public sealed class AppException : Exception
{
    public AppErrorCode Code { get; }

    private AppException(AppErrorCode code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public static AppException NotFound(string message, Exception? innerException = null)
        => new(AppErrorCode.NotFound, message, innerException);

    public static AppException NoAvailableSeats(string message, Exception? innerException = null)
        => new(AppErrorCode.NoAvailableSeats, message, innerException);
}
