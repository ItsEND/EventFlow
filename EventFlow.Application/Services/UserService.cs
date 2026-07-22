using EventFlow.Application.Abstractions.Repositories;
using EventFlow.Application.Abstractions.Security;
using EventFlow.Application.Abstractions.Services;
using EventFlow.Application.Dtos.Users;
using EventFlow.Application.Exceptions;
using EventFlow.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Application.Services;

public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator) : IUserService
{


    public async Task RegisterAsync(RegisterUserModel model,  CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Login))
        {
            throw new ValidationException("Логин не может быть пустым.");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            throw new ValidationException("Пароль не может быть пустым.");
        }

        var role = ParseRole(model.Role);


        var existingUser = await userRepository.GetByLoginAsync(model.Login, ct);

        if (existingUser is not null)
        {
            throw new ValidationException("Пользователь с таким логином уже существует.");
        }

        var passwordHash = passwordHasher.Hash(model.Password);
        var user = User.Create(model.Login, passwordHash, role);

        userRepository.Add(user);
        await userRepository.SaveChangesAsync(ct);
    }

    public async Task<string> LoginAsync(string login, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            throw AppException.NotFound("Неверный логин или пароль.");
        }

        var user = await userRepository.GetByLoginAsync(login, ct);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            throw AppException.NotFound("Неверный логин или пароль.");
        }

        return jwtTokenGenerator.Generate(user);
    }

    private static UserRole ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return UserRole.User;
        }

        if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole) || !Enum.IsDefined(parsedRole))
        {
            throw new ValidationException("Допустимые роли: User и Admin.");
        }

        return parsedRole;
    }
}
