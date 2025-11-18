namespace AuthService.Contracts.Responses;

public record RefreshTokensResponse(string AccessToken, Guid RefreshToken);