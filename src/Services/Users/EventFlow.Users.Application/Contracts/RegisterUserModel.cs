namespace EventFlow.Users.Application.Contracts
{
    public record class RegisterUserModel
    {
        public required string Login { get; init; }
        public required string Password { get; init; }

        public string? Role { get; init; }
    }
}
