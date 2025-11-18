using System.Net.Http.Headers;
using AuthService.Application.Commands.Users.CheckEmail;
using AuthService.Application.Commands.Users.Login;
using AuthService.Application.Commands.Users.Logout;
using AuthService.Application.Commands.Users.RefreshTokens;
using AuthService.Application.Commands.Users.Register;
using AuthService.Contracts.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Presentation.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    /// <summary>
    /// Авторизация по email и паролю. Возвращает пару токенов и идентификатор сессии.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await handler.Handle(command, ct);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Обновление пары токенов по действующему refresh-токену (ротация).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokensRequest request,
        [FromServices] RefreshTokensHandler handler,
        CancellationToken ct)
    {
        var authorization = Request.Headers.Authorization.ToString();

        if (!AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !string.Equals(header.Scheme, JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return Unauthorized("Заголовок авторизации отсутствует или недействителен");
        }

        var accessToken = header.Parameter;

        var command = new RefreshTokensCommand(accessToken, request.RefreshToken);
        var result = await handler.Handle(command, ct);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Логаут. Инвалидирует текущую сессию (или все, если allDevices = true).
    /// </summary>
    [Permission("auth.service")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] LogoutHandler handler,
        CancellationToken ct)
    {
        var authorization = Request.Headers.Authorization.ToString();

        if (!AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !string.Equals(header.Scheme, JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return Unauthorized("Заголовок авторизации отсутствует или недействителен");
        }

        var accessToken = header.Parameter;

        var command = new LogoutCommand(accessToken, request.RefreshToken, request.AllDevices);

        var result = await handler.Handle(command, ct);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Регистрация пользователя. Возвращает пару токенов (как и login).
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        [FromServices] RegisterHandler handler,
        CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FullName);
        var result = await handler.Handle(command, ct);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }

    /// <summary>Проверка существования пользователя по email.</summary>
    [HttpPost("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmail(
        [FromBody] CheckEmailRequest request,
        [FromServices] CheckEmailHandler handler,
        CancellationToken ct)
    {
        var command = new CheckEmailCommand(request.Email);
        var result = await handler.Handle(command, ct);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }
}