namespace AuthService.Application.Commands.Users.CheckEmail;

public record CheckEmailCommand(string Email) : ICommand;