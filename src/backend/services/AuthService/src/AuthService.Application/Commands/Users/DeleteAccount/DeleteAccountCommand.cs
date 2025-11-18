namespace AuthService.Application.Commands.Users.DeleteAccount;

public record DeleteAccountCommand(string AccessToken) : ICommand;