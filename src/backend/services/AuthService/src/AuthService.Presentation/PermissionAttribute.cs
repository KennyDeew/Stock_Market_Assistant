using Microsoft.AspNetCore.Authorization;

namespace AuthService.Presentation;

public sealed class PermissionAttribute : AuthorizeAttribute
{
    public const string PREFIX = "permission:";

    public string Code { get; }

    public PermissionAttribute(string code)
    {
        Code = code;
        Policy = PREFIX + code;
    }
}