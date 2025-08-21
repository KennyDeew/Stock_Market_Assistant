using Microsoft.AspNetCore.Authorization;

namespace AuthService.Presentation;

public class PermissionAttribute : AuthorizeAttribute, IAuthorizationRequirement
{
    public string Code { get; }

    public PermissionAttribute(string code)
        : base(policy: PermissionPolicyProvider.PolicyPrefix + code)
    {
        Code = code;
    }
}