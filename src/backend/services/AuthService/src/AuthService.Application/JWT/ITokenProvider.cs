using System.Security.Claims;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AuthService.Application.JWT;
public interface ITokenProvider
{
    Task<JwtTokenResult> GenerateAccessToken(User user, CancellationToken cancellationToken);

    Task<Guid> GenerateRefreshToken(User user, Guid accessTokenJti, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<Claim>, Error>> GetUserClaims(string jwtToken, CancellationToken cancellationToken);
}