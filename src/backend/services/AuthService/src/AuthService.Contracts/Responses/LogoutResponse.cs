namespace AuthService.Contracts.Responses;

public sealed record LogoutResponse(bool IsSuccess, string Message);