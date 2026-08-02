namespace EventFlow.Users.Infrastructure.Security;
/// <summary>
/// Настройки создания и проверки JWT.
/// </summary>
public sealed record JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int LifetimeMinutes { get; init; }

}
