namespace AuthService.Application.Commands.Users.Logout;

public record LogoutCommand(string AccessToken, Guid RefreshToken, bool AllDevices) : ICommand;