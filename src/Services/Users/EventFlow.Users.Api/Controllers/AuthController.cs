using EventFlow.Users.Application.Contracts;
using EventFlow.Users.Api.Contracts.Auth;
using EventFlow.Users.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace EventFlow.Users.Api.Controllers;


/// <summary>
/// Регистрация и вход пользователей.
/// </summary>
[ApiController]
[Route("auth")]
[AllowAnonymous]
public class AuthController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var model = new RegisterUserModel
        {
            Login = request.Login,
            Password = request.Password,
            Role = request.Role
        };

        await userService.RegisterAsync(model, ct);

        return NoContent();
    }

    /// <summary>
    /// Проверяет учётные данные и возвращает JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var token = await userService.LoginAsync(request.Login,request.Password, ct);

        return Ok (new TokenResponse{ Token = token });
    }
}
