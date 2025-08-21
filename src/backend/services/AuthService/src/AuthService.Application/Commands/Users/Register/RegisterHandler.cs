// чтобы видеть RegisterCommand
using AuthService.Application.JWT;
using AuthService.Contracts.Responses;
using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace AuthService.Application.Commands.Users.Register;

public class RegisterHandler : ICommandHandler<RegisterResponse, RegisterCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ITokenProvider _tokenProvider;

    public RegisterHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ITokenProvider tokenProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<RegisterResponse, ErrorList>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Failure<RegisterResponse, ErrorList>(Errors.General.ValueIsRequired("Email").ToErrorList());
        if (string.IsNullOrWhiteSpace(command.Password))
            return Result.Failure<RegisterResponse, ErrorList>(Errors.General.ValueIsRequired("Password").ToErrorList());
        if (string.IsNullOrWhiteSpace(command.FullName))
            return Result.Failure<RegisterResponse, ErrorList>(Errors.General.ValueIsRequired("FullName").ToErrorList());

        var existing = await _userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            return Result.Failure<RegisterResponse, ErrorList>(Errors.General.AlreadyExist("Пользователь").ToErrorList());

        // ✅ создаём через фабрику
        var user = User.Create(command.Email, command.Email);

        var create = await _userManager.CreateAsync(user, command.Password);
        if (!create.Succeeded)
        {
            var errs = create.Errors
                .Select(e => Error.Validation($"core.identity.create.{e.Code}".ToLowerInvariant(), e.Description))
                .ToList();
            return Result.Failure<RegisterResponse, ErrorList>(new ErrorList(errs));
        }

        const string defaultRole = "User";
        var role = await _roleManager.FindByNameAsync(defaultRole);
        if (role is not null)
        {
            var addRole = await _userManager.AddToRoleAsync(user, defaultRole);
            if (!addRole.Succeeded)
            {
                var errs = addRole.Errors
                    .Select(e => Error.Validation($"core.identity.role.{e.Code}".ToLowerInvariant(), e.Description))
                    .ToList();
                return Result.Failure<RegisterResponse, ErrorList>(new ErrorList(errs));
            }
        }

        var access = await _tokenProvider.GenerateAccessToken(user, cancellationToken);
        var refresh = await _tokenProvider.GenerateRefreshToken(user, access.Jti, cancellationToken);

        return Result.Success<RegisterResponse, ErrorList>(new RegisterResponse(access.AccessToken, refresh));
    }
}