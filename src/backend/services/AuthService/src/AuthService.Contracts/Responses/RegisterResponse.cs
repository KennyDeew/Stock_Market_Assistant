namespace AuthService.Contracts.Responses;

public record RegisterResponse(string AccessToken, Guid RefreshToken);