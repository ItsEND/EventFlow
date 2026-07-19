using EventFlow.Domain.Models;

namespace EventFlow.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
