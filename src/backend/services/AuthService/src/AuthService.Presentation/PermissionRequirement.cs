using Microsoft.AspNetCore.Authorization;

namespace AuthService.Presentation
{
    public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;
}