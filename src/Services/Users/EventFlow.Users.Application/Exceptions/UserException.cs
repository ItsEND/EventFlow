namespace EventFlow.Users.Application.Exceptions;

/// <summary>
/// Единое исключение слоя Application для передачи ошибок во внешние слои.
/// </summary>
public sealed class UserException : Exception
{
    public UserErrorCode Code { get; }

    private UserException(UserErrorCode code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public static UserException InvalidCredentials(string message, Exception? innerException = null)
        => new(UserErrorCode.InvalidCredentials, message, innerException);

    public static UserException LoginAlreadyExists(string message, Exception? innerException = null)
        => new(UserErrorCode.LoginAlreadyExists, message, innerException);
    public static UserException Forbidden(string message, Exception? innerException = null)
        => new(UserErrorCode.Forbidden, message, innerException);
    
}
