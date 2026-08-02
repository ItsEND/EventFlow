using EventFlow.Events.Domain.Models;

namespace EventFlow.Events.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
