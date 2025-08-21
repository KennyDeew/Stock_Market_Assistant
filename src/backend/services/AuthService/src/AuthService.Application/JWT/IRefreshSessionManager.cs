using AuthService.Domain;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AuthService.Application.JWT;

public interface IRefreshSessionManager
{
    Task<Result<RefreshSession, Error>> GetByRefreshToken(Guid refreshToken, CancellationToken cancellationToken);

    void Delete(RefreshSession refreshSession);
}