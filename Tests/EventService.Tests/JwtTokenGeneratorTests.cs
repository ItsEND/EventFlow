using EventFlow.Users.Domain.Models;
using EventFlow.Users.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventService.Tests;

public class JwtTokenGeneratorTests
{
    private const string Secret = "jwt-generator-test-secret-key-long-enough-2026";

    [Fact]
    public void Generate_ShouldCreateValidTokenWithUserClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = Secret,
            Issuer = "EventFlow.Tests",
            Audience = "EventFlow.Tests.Client",
            LifetimeMinutes = 60
        });
        var generator = new JwtTokenGenerator(options);
        var user = User.Create("admin", "PASSWORD_HASH", UserRole.Admin);

        var token = generator.Generate(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)),
            ValidateIssuer = true,
            ValidIssuer = "EventFlow.Tests",
            ValidateAudience = true,
            ValidAudience = "EventFlow.Tests.Client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        var principal = new JwtSecurityTokenHandler().ValidateToken(
            token,
            validationParameters,
            out var validatedToken);

        Assert.NotNull(validatedToken);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Login, principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(UserRole.Admin.ToString(), principal.FindFirst(ClaimTypes.Role)?.Value);
    }
}
