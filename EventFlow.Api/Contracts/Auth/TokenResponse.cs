namespace EventFlow.Api.Contracts.Auth;

/// <summary>
/// Ответ с JWT.
/// </summary>
public record class TokenResponse
{
    /// <summary>
    /// JWT пользователя.
    /// </summary>
    public required string Token { get; init; }
}
