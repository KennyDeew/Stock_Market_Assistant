namespace AuthService.Application.Commands.Users.Login;

public record LoginCommand(string Email, string Password) : ICommand;