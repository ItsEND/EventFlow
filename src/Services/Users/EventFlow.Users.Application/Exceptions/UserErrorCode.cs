namespace EventFlow.Users.Application.Exceptions;

/// <summary>
/// Коды ошибок, которые слой Application отдает внешним слоям.
/// </summary>
public enum UserErrorCode
{
    InvalidCredentials,
    LoginAlreadyExists,
    Forbidden
}
