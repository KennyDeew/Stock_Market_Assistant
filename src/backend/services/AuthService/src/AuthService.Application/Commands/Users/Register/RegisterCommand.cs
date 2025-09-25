namespace AuthService.Application.Commands.Users.Register;

public record RegisterCommand(string Email, string Password, string FullName) : ICommand;