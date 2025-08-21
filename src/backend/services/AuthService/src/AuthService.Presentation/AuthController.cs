using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Application.Commands.Users.CheckEmail;
using AuthService.Application.Commands.Users.Login;
using AuthService.Application.Commands.Users.Logout;
using AuthService.Application.Commands.Users.RefreshTokens;
using AuthService.Application.Commands.Users.Register;
using AuthService.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AuthService.Presentation;

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
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RefreshTokensCommand(request.AccessToken, request.RefreshToken),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Логаут. Инвалидирует текущую сессию (или все, если allDevices = true).
    /// </summary>
    [HttpPost("logout")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        [FromServices] LogoutHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new LogoutCommand(request.AllDevices),
            cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RegisterCommand(request.Email, request.Password, request.FullName),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToResponse();
        }

        return Ok(result.Value);
    }

    /// <summary>Проверка существования пользователя по email</summary>
    [HttpPost("check-email")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmail(
        [FromBody] CheckEmailRequest request,
        [FromServices] CheckEmailHandler handler,
        CancellationToken cancellationToken)
    {
        var cmd = new CheckEmailCommand(request.Email);
        var result = await handler.Handle(cmd, cancellationToken);

        if (result.IsFailure) return result.Error.ToResponse();
        return Ok(result.Value); // CheckEmailResponse
    }
}