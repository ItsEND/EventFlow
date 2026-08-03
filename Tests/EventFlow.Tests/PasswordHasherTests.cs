using EventFlow.Users.Infrastructure.Security;

namespace EventFlow.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void Hash_ShouldReturnHashInsteadOfOriginalPassword()
    {
        const string password = "secret123";

        var hash = _passwordHasher.Hash(password);

        Assert.NotEqual(password, hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        const string password = "secret123";
        var hash = _passwordHasher.Hash(password);

        Assert.True(_passwordHasher.Verify(password, hash));
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        var hash = _passwordHasher.Hash("correct-password");

        Assert.False(_passwordHasher.Verify("incorrect-password", hash));
    }

    [Fact]
    public void Hash_ShouldReturnSameHash_ForSamePassword()
    {
        const string password = "secret123";

        Assert.Equal(_passwordHasher.Hash(password), _passwordHasher.Hash(password));
    }
}
