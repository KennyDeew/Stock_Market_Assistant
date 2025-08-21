using System.Security.Claims;
using AuthService.Contracts.Models;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Presentation;

public sealed class PermissionRequirementHandler : AuthorizationHandler<PermissionAttribute>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAttribute requirement)
    {
        var hasPermission = context.User.Claims
            .Where(c => c.Type == CustomClaims.Permission)
            .Select(c => c.Value)
            .Any(code => string.Equals(code, requirement.Code, StringComparison.Ordinal));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
