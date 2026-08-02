using EventFlow.Events.Infrastructure.Security;

namespace EventService.Tests;

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

        var result = _passwordHasher.Verify(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        var hash = _passwordHasher.Hash("correct-password");

        var result = _passwordHasher.Verify("incorrect-password", hash);

        Assert.False(result);
    }

    [Fact]
    public void Hash_ShouldReturnSameHash_ForSamePassword()
    {
        const string password = "secret123";

        var firstHash = _passwordHasher.Hash(password);
        var secondHash = _passwordHasher.Hash(password);

        Assert.Equal(firstHash, secondHash);
    }

}
