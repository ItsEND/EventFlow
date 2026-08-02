using System.ComponentModel.DataAnnotations;

namespace EventFlow.Events.Api.Contracts.Auth;
/// <summary>
/// Запрос регистрации пользователя.
/// </summary>
public record class RegisterRequest
{
    /// <summary>
    /// Уникальный логин.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Login { get; init; } = string.Empty;

    /// <summary>
    /// Пароль пользователя.
    /// </summary>
    [Required]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Роль User или Admin. По умолчанию User.
    /// </summary>
    public string? Role { get; init; }
}
