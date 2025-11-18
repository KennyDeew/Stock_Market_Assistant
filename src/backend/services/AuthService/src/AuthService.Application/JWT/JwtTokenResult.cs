namespace AuthService.Application.JWT;

public record JwtTokenResult(string AccessToken, Guid Jti);