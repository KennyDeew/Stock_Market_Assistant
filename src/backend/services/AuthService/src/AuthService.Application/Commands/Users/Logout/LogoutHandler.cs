using AuthService.Application.Database;
using AuthService.Application.JWT;
using AuthService.Contracts.Models;
using AuthService.Contracts.Responses;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AuthService.Application.Commands.Users.Logout;

public class LogoutHandler : ICommandHandler<LogoutResponse, LogoutCommand>
{
    private readonly IRefreshSessionManager _refreshSessionManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUnitOfWork _uow;

    public LogoutHandler(
        IRefreshSessionManager refreshSessionManager,
        ITokenProvider tokenProvider,
        IUnitOfWork uow)
    {
        _refreshSessionManager = refreshSessionManager;
        _tokenProvider = tokenProvider;
        _uow = uow;
    }

    public async Task<Result<LogoutResponse, ErrorList>> Handle(
        LogoutCommand command,
        CancellationToken ct)
    {
        var claimsResult = await _tokenProvider.GetUserClaims(command.AccessToken, ct);
        if (claimsResult.IsFailure)
            return claimsResult.Error.ToErrorList();

        var claims = claimsResult.Value;

        if (!Guid.TryParse(claims.FirstOrDefault(c => c.Type == CustomClaims.Id)?.Value, out var userId) || userId == Guid.Empty)
            return Errors.General.Failure().ToErrorList();

        if (!Guid.TryParse(claims.FirstOrDefault(c => c.Type == CustomClaims.Jti)?.Value, out var jti) || jti == Guid.Empty)
            return Errors.General.Failure().ToErrorList();

        var strategy = _uow.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _uow.BeginTransaction(ct);
            try
            {
                if (command.AllDevices)
                {
                    var delAll = await _refreshSessionManager.DeleteAllByUserId(userId, ct);
                    if (delAll.IsFailure)
                        return Result.Failure<LogoutResponse, ErrorList>(delAll.Error.ToErrorList());
                }
                else
                {
                    var sessionResult = await _refreshSessionManager.GetByRefreshToken(command.RefreshToken, ct);
                    if (sessionResult.IsFailure)
                        return Result.Failure<LogoutResponse, ErrorList>(sessionResult.Error.ToErrorList());

                    var session = sessionResult.Value;

                    if (session.UserId != userId || session.Jti != jti)
                        return Result.Failure<LogoutResponse, ErrorList>(Errors.Tokens.InvalidToken().ToErrorList());

                    _refreshSessionManager.Delete(session, ct);
                    await _uow.SaveChanges(ct);
                }

                await tx.CommitAsync(ct);

                var response = new LogoutResponse(true, "Вы вышли из системы");
                return Result.Success<LogoutResponse, ErrorList>(response);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }
}