namespace AuthService.Application.JWT;

public record JwtAccessTokenResult(string AccessToken, Guid Jti, DateTime ExpiresAtUtc);