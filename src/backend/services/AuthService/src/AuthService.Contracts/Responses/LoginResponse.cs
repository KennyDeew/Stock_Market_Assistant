namespace AuthService.Contracts.Responses;

public record LoginResponse(string AccessToken, Guid RefreshToken);