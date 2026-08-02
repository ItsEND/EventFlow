using System.ComponentModel.DataAnnotations;

namespace EventFlow.Events.Api.Contracts.Auth;
/// <summary>
/// Запрос входа пользователя.
/// </summary>
public record class LoginRequest
{
    /// <summary>
    /// Логин пользователя.
    /// </summary>
    [Required]
    public string Login { get; init; } = string.Empty;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    [Required]
    public string Password { get; init; } = string.Empty;
}
