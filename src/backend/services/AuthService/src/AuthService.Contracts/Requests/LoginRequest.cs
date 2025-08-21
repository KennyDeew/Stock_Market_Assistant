namespace AuthService.Contracts.Requests;

public record LoginRequest(string Email, string Password);