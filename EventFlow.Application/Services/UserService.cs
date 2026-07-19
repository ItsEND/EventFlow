using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Application.Abstractions.Security;
using EventFlow.Application.Abstractions.Services;
using EventFlow.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Application.Services;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
{
    public async Task RegisterAsync(string login, string password, UserRole role = UserRole.User, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            throw new ValidationException("Логин не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("Пароль не может быть пустым.");
        }

        var existingUser = await userRepository.GetByLoginAsync(login, ct);

        if (existingUser is not null)
        {
            throw new ValidationException("Пользователь с таким логином уже существует.");
        }

        var passwordHash = passwordHasher.Hash(password);
        var user = User.Create(login, passwordHash, role);

        userRepository.Add(user);
        await userRepository.SaveChangesAsync(ct);
    }
}
