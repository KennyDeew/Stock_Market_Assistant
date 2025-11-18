namespace AuthService.Contracts.Responses;

public sealed record LoginResponse(string AccessToken, Guid RefreshToken);