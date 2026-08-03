using EventFlow.Users.Domain.Models;

namespace EventFlow.Users.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
