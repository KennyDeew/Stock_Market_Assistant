namespace AuthService.Contracts.Requests;

public record LogoutRequest(string AccessToken, Guid RefreshToken, bool AllDevices);