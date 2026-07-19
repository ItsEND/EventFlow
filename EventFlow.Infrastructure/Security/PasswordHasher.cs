using EventFlow.Application.Abstractions.Security;
using System.Security.Cryptography;
using System.Text;

namespace EventFlow.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Пароль не может быть пустым.",
            nameof(password));
        }

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(passwordBytes);

        return Convert.ToHexString(hashBytes);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }
        var actualHash = Hash(password);
        return string.Equals(actualHash, passwordHash, StringComparison.Ordinal);
    }

}
