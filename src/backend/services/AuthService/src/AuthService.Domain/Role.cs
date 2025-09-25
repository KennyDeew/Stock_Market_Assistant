using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain;

public class Role : IdentityRole<Guid>
{
    public List<RolePermission> RolePermissions { get; set; } = [];
}