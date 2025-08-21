namespace AuthService.Application.JWT;

public record RefreshTokenResult(Guid RefreshToken, Guid AccessTokenJti, DateTime ExpiresAtUtc);