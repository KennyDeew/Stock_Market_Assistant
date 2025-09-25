namespace AuthService.Infrastructure.Postgres.Options;

public sealed class RolePermissionOptions
{
    public const string SECTION_NAME = "RolePermission";

    public Dictionary<string, string[]?>? Permissions { get; init; }

    public Dictionary<string, string[]?>? Roles { get; init; }
}