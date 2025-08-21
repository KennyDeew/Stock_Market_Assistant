namespace AuthService.Application.Commands.Users.Logout;

public record LogoutCommand(bool AllDevices) : ICommand;