using Microsoft.AspNetCore.Authorization;

namespace AuthService.Presentation
{
    public sealed class PermissionRequirementHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var has = context.User
                .FindAll("Permission")
                .Any(c => string.Equals(c.Value, requirement.Code, StringComparison.OrdinalIgnoreCase));

            if (has)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}