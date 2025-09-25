using AuthService.Application.JWT;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AuthService.Infrastructure.Postgres.IdentityManagers
{
    public class RefreshSessionManager(AccountsWriteDbContext db) : IRefreshSessionManager
    {
        public async Task<Result<RefreshSession, Error>> GetByRefreshToken(
            Guid refreshToken, CancellationToken ct)
        {
            if (refreshToken == Guid.Empty)
                return Errors.General.ValueIsInvalid(nameof(refreshToken));

            var session = await db.RefreshSessions
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, ct);

            if (session is null)
                return Errors.General.NotFound(name: "Сессия обновления");

            if (session.ExpiresIn <= DateTime.UtcNow)
                return Errors.Tokens.ExpiredToken();

            return session;
        }

        public async Task<Result<bool, Error>> DeleteAllByUserId(Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty)
                return Errors.General.ValueIsInvalid(nameof(userId));

            var rows = await db.RefreshSessions
                .Where(r => r.UserId == userId)
                .ExecuteDeleteAsync(ct);

            return rows > 0;
        }

        public void Delete(RefreshSession refreshSession, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(refreshSession);

            // db.Attach(refreshSession);
            db.RefreshSessions.Remove(refreshSession);
        }
    }
}