namespace AuthService.Contracts.Requests;

public record RegisterRequest(string FullName, string Email, string Password);