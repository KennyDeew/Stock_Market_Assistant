using System.Net.Http.Headers;
using AuthService.Application.Commands.Users.DeleteAccount;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Presentation.Controllers;

[ApiController]
[Route("api/v1/account")]
public sealed class AccountController: ControllerBase
{
    /// <summary>
    /// Удаление текущего аккаунта (по access token из заголовка Authorization).
    /// </summary>
    [Permission("auth.service")]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount(
        [FromServices] DeleteAccountHandler handler,
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

        var command = new DeleteAccountCommand(accessToken);

        var result = await handler.Handle(command, ct);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(result.Value);
    }
}