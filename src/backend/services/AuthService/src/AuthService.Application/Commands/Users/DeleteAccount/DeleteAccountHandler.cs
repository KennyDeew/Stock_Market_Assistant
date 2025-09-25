using System.Text;
using System.Text.Json;
using AuthService.Application.Database;
using AuthService.Application.JWT;
using AuthService.Contracts.Models;
using AuthService.Contracts.Responses;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace AuthService.Application.Commands.Users.DeleteAccount;

public class DeleteAccountHandler : ICommandHandler<DeleteAccountResponse, DeleteAccountCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRefreshSessionManager _refreshSessions;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(
        UserManager<User> userManager,
        IRefreshSessionManager refreshSessions,
        IUnitOfWork uow,
        ILogger<DeleteAccountHandler> logger)
    {
        _userManager = userManager;
        _refreshSessions = refreshSessions;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<DeleteAccountResponse, ErrorList>> Handle(
        DeleteAccountCommand command,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.AccessToken))
        {
            return Result.Failure<DeleteAccountResponse, ErrorList>(
                Errors.General.ValueIsRequired("AccessToken"));
        }

        var userIdFromToken = TryGetUserIdFromJwt(command.AccessToken);
        if (userIdFromToken.IsFailure)
        {
            return Result.Failure<DeleteAccountResponse, ErrorList>(userIdFromToken.Error);
        }

        var userId = userIdFromToken.Value;

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure<DeleteAccountResponse, ErrorList>(
                Errors.General.NotFound(userId.ToString(), "Пользователь"));
        }

        return await _uow
            .CreateExecutionStrategy()
            .ExecuteAsync(
                state: user,
                operation: async (DbContext db, User u, CancellationToken innerCt) =>
                {
                    await using var tx = await _uow.BeginTransaction(innerCt);

                    var rs = await _refreshSessions.DeleteAllByUserId(u.Id, innerCt);
                    if (rs.IsFailure)
                    {
                        await tx.RollbackAsync(innerCt);
                        _logger.LogError("DeleteAllByUserId failed for {UserId}", u.Id);
                        return Result.Failure<DeleteAccountResponse, ErrorList>(Errors.General.Failure());
                    }

                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $"DELETE FROM accounts.user_roles WHERE user_id = {u.Id};",
                        innerCt);

                    var leftCount = await db.Database
                        .SqlQuery<int>($"SELECT COUNT(*) AS \"Value\" FROM accounts.user_roles WHERE user_id = {u.Id}")
                        .SingleAsync(innerCt);

                    if (leftCount > 0)
                    {
                        await tx.RollbackAsync(innerCt);
                        _logger.LogError(
                            "Cannot delete user {UserId}: roles still attached (left={Left})",
                            u.Id,
                            leftCount);

                        return Result.Failure<DeleteAccountResponse, ErrorList>(Errors.General.Failure());
                    }

                    var del = await _userManager.DeleteAsync(u);
                    if (!del.Succeeded)
                    {
                        await tx.RollbackAsync(innerCt);
                        _logger.LogError(
                            "User delete failed for {UserId}: {Errors}",
                            u.Id,
                            string.Join(", ", del.Errors.Select(e => $"{e.Code}:{e.Description}")));

                        return Result.Failure<DeleteAccountResponse, ErrorList>(Errors.General.Failure());
                    }

                    await _uow.SaveChanges(innerCt);
                    await tx.CommitAsync(innerCt);

                    _logger.LogInformation("User {UserId} deleted", u.Id);

                    return Result.Success<DeleteAccountResponse, ErrorList>(
                        new DeleteAccountResponse(u.Id.ToString()));
                },
                verifySucceeded: null,
                cancellationToken: ct);
    }

    private static Result<Guid, Error> TryGetUserIdFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2)
                return Errors.Tokens.InvalidToken();

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(payloadJson);

            var claimName = string.IsNullOrWhiteSpace(CustomClaims.Id)
                ? "Id"
                : CustomClaims.Id;

            if (!doc.RootElement.TryGetProperty(claimName, out var idProp))
                return Errors.Tokens.InvalidToken();

            var idStr = idProp.GetString();
            if (string.IsNullOrWhiteSpace(idStr))
                return Errors.Tokens.InvalidToken();

            if (!Guid.TryParse(idStr, out var guid))
                return Errors.Tokens.InvalidToken();

            return guid;
        }
        catch
        {
            return Errors.Tokens.InvalidToken();
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }

        return Convert.FromBase64String(s);
    }
}