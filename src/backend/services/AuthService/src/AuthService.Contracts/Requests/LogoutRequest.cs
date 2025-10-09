namespace AuthService.Contracts.Requests;

public sealed record LogoutRequest(Guid RefreshToken, bool AllDevices);