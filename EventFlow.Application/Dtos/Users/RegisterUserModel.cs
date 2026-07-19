using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.Application.Dtos.Users
{
    public record class RegisterUserModel
    {
        public required string Login { get; init; }
        public required string Password { get; init; }

        public string? Role { get; init;  }
    }
}
