using AuthService.Contracts.Responses;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AuthService.Application.Commands.Users.Logout;

public class LogoutHandler : ICommandHandler<LogoutResponse, LogoutCommand>
{
    public Task<Result<LogoutResponse, ErrorList>> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}