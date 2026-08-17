using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace PlatformTemplate.BuildingBlocks.Authentication;

public static class PlatformAuthenticationExtensions
{
    public static WebApplicationBuilder AddPlatformAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
        var secret = jwtSection[nameof(JwtOptions.Secret)]
            ?? throw new InvalidOperationException("Jwt:Secret не настроен.");
        var issuer = jwtSection[nameof(JwtOptions.Issuer)]
            ?? throw new InvalidOperationException("Jwt:Issuer не настроен.");
        var audience = jwtSection[nameof(JwtOptions.Audience)]
            ?? throw new InvalidOperationException("Jwt:Audience не настроен.");

        if (secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret должен содержать не менее 32 символов.");
        }

        builder.Services.AddOptions<JwtOptions>()
            .Bind(jwtSection)
            .Validate(options => options.Secret.Length >= 32, "Jwt:Secret должен содержать не менее 32 символов.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer обязателен.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience обязателен.")
            .ValidateOnStart();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorizationBuilder();

        return builder;
    }

    public static WebApplication UsePlatformAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
