using AuthService.Domain;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AuthService.Application.JWT;

public interface IRefreshSessionManager
{
    Task<Result<RefreshSession, Error>> GetByRefreshToken(Guid refreshToken, CancellationToken cancellationToken);

    Task<Result<bool, Error>> DeleteAllByUserId(Guid userId, CancellationToken ct);

    void Delete(RefreshSession refreshSession, CancellationToken cancellationToken);
}