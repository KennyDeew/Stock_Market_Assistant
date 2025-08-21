using AuthService.Application.JWT;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AuthService.Infrastructure.Postgres.IdentityManagers
{
    public class RefreshSessionManager : IRefreshSessionManager
    {
        private readonly AccountsWriteDbContext _ctx;

        public RefreshSessionManager(AccountsWriteDbContext accountsWriteContext)
        {
            ArgumentNullException.ThrowIfNull(accountsWriteContext);
            _ctx = accountsWriteContext;
        }

        public async Task<Result<RefreshSession, Error>> GetByRefreshToken(
            Guid refreshToken,
            CancellationToken cancellationToken)
        {
            if (refreshToken == Guid.Empty)
            {
                return Errors.General.ValueIsInvalid("RefreshToken");
            }

            RefreshSession? session = await _ctx.RefreshSessions
                .AsNoTracking()
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, cancellationToken)
                .ConfigureAwait(false);

            if (session is null)
            {
                return Errors.General.NotFound(name: "Сессия обновления");
            }

            if (session.ExpiresIn <= DateTime.UtcNow)
            {
                return Errors.Tokens.ExpiredToken();
            }

            return session;
        }

        public void Delete(RefreshSession refreshSession)
        {
            ArgumentNullException.ThrowIfNull(refreshSession);
            _ctx.RefreshSessions.Remove(refreshSession);
        }
    }
}