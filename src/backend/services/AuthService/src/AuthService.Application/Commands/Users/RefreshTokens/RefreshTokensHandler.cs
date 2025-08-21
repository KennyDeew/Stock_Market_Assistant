using AuthService.Application.Database;
using AuthService.Application.JWT;
using AuthService.Contracts.Models;
using AuthService.Contracts.Responses;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AuthService.Application.Commands.Users.RefreshTokens;

public class RefreshTokensHandler : ICommandHandler<RefreshTokensResponse, RefreshTokensCommand>
{
    private readonly IRefreshSessionManager _refreshSessionManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUnitOfWork _uow;

    public RefreshTokensHandler(
        IRefreshSessionManager refreshSessionManager,
        ITokenProvider tokenProvider,
        IUnitOfWork uow)
    {
        _refreshSessionManager = refreshSessionManager;
        _tokenProvider = tokenProvider;
        _uow = uow;
    }

    public async Task<Result<RefreshTokensResponse, ErrorList>> Handle(
        RefreshTokensCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1) Читаем сессию по refreshToken
        var sessionResult = await _refreshSessionManager.GetByRefreshToken(command.RefreshToken, cancellationToken);
        if (sessionResult.IsFailure)
            return Result.Failure<RefreshTokensResponse, ErrorList>(sessionResult.Error.ToErrorList());

        var session = sessionResult.Value;

        if (session.ExpiresIn <= DateTime.UtcNow)
            return Result.Failure<RefreshTokensResponse, ErrorList>(Errors.Tokens.ExpiredToken().ToErrorList());

        // 2) Валидируем access token (без Lifetime) и сопоставляем userId/jti
        var claimsResult = await _tokenProvider.GetUserClaims(command.AccessToken, cancellationToken);
        if (claimsResult.IsFailure)
            return Result.Failure<RefreshTokensResponse, ErrorList>(Errors.Tokens.InvalidToken().ToErrorList());

        var claims = claimsResult.Value;

        var userIdStr = claims.FirstOrDefault(c => c.Type == CustomClaims.Id)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId) || userId == Guid.Empty)
            return Result.Failure<RefreshTokensResponse, ErrorList>(Errors.General.Failure().ToErrorList());

        if (session.UserId != userId)
            return Result.Failure<RefreshTokensResponse, ErrorList>(Errors.Tokens.InvalidToken().ToErrorList());

        var jtiStr = claims.FirstOrDefault(c => c.Type == CustomClaims.Jti)?.Value;
        if (!Guid.TryParse(jtiStr, out var jti) || jti == Guid.Empty)
            return Result.Failure<RefreshTokensResponse, ErrorList>(Errors.General.Failure().ToErrorList());

        if (session.Jti != jti)
            return Result.Failure<RefreshTokensResponse, ErrorList>(Errors.Tokens.InvalidToken().ToErrorList());

        // 3) Транзакция ротации refresh-сессии
        await using var tx = await _uow.BeginTransaction(cancellationToken);
        try
        {
            _refreshSessionManager.Delete(session);
            await _uow.SaveChanges(cancellationToken); // фиксируем удаление

            var accessToken = await _tokenProvider.GenerateAccessToken(session.User, cancellationToken);
            var newRefreshToken = await _tokenProvider.GenerateRefreshToken(session.User, accessToken.Jti, cancellationToken);

            await _uow.SaveChanges(cancellationToken); // фиксируем вставку новой сессии

            await tx.CommitAsync(cancellationToken);

            var response = new RefreshTokensResponse(accessToken.AccessToken, newRefreshToken);
            return Result.Success<RefreshTokensResponse, ErrorList>(response);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}