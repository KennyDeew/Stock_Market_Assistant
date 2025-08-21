using AuthService.Contracts.Responses;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace AuthService.Application.Commands.Users.CheckEmail;

public sealed class CheckEmailHandler : ICommandHandler<CheckEmailResponse, CheckEmailCommand>
{
    private readonly UserManager<User> _userManager;

    public CheckEmailHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<CheckEmailResponse, ErrorList>> Handle(
        CheckEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure<CheckEmailResponse, ErrorList>(Errors.General.ValueIsRequired("Email").ToErrorList());
        }

        var user = await _userManager.FindByEmailAsync(command.Email);
        var exists = user is not null;

        return Result.Success<CheckEmailResponse, ErrorList>(new CheckEmailResponse(exists));
    }
}