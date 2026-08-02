using System.ComponentModel.DataAnnotations;

namespace EventFlow.Users.Domain.Models;

public class User
{
    public Guid Id { get; init; }
    public string Login { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    private User()
    {
        //Для Ef Core
    }

    private User(Guid id, string login, string passwordHash, UserRole role)
    {
        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User Create(string login, string passwordHash, UserRole role = UserRole.User)
    {
        return string.IsNullOrWhiteSpace(login)
            ? throw new ValidationException("Логин пользователя не может быть пустым")
            : string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ValidationException("Пароль пользователя не может быть пустым")
            : new User(Guid.NewGuid(), login.Trim(), passwordHash, role);
    }


}
