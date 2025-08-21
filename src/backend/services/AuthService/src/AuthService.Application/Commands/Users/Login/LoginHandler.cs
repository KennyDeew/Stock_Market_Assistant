using AuthService.Application.JWT;
using AuthService.Contracts.Responses;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace AuthService.Application.Commands.Users.Login;

public class LoginHandler : ICommandHandler<LoginResponse, LoginCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<User> userManager,
        ITokenProvider tokenProvider,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public async Task<Result<LoginResponse, ErrorList>> Handle(
        LoginCommand command, CancellationToken cancellationToken = default)
    {
        Error invalid = Errors.User.InvalidCredentials();

        // ищем пользователя
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: unknown email {Email}", command.Email);
            return Result.Failure<LoginResponse, ErrorList>(invalid.ToErrorList());
        }

        // проверяем пароль
        var passwordOk = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordOk)
        {
            _logger.LogWarning("Login failed: bad password for user {UserId}", user.Id);
            return Result.Failure<LoginResponse, ErrorList>(invalid.ToErrorList());
        }

        // генерим пару токенов
        var accessToken = await _tokenProvider.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenProvider.GenerateRefreshToken(user, accessToken.Jti, cancellationToken);

        var response = new LoginResponse(accessToken.AccessToken, refreshToken);

        _logger.LogInformation("Login success for user {UserId}", user.Id);
        return Result.Success<LoginResponse, ErrorList>(response);
    }
}