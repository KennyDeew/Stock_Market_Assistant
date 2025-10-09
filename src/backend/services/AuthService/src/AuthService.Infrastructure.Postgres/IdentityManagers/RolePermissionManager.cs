using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres.IdentityManagers;

public class RolePermissionManager
{
    private readonly PostgresDbContext _db;

    public RolePermissionManager(PostgresDbContext accountsWriteContext)
    {
        ArgumentNullException.ThrowIfNull(accountsWriteContext);
        _db = accountsWriteContext;
    }

    public async Task AddRangeIfExist(
        Guid roleId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("roleId is empty.", nameof(roleId));
        }

        ArgumentNullException.ThrowIfNull(permissions);

        List<string> codes = permissions
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (codes.Count == 0)
        {
            return;
        }

        List<Permission> existingPermissions = await _db.Permissions
            .AsNoTracking()
            .Where(p => codes.Contains(p.Code))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        HashSet<string> existingCodes = new HashSet<string>(
            existingPermissions.Select(p => p.Code),
            StringComparer.Ordinal);

        List<string> missing = codes
            .Where(c => !existingCodes.Contains(c))
            .ToList();

        if (missing.Count > 0)
        {
            string message = string.Concat(
                "Permission code(s) not found: ",
                string.Join(", ", missing),
                ".");
            throw new ApplicationException(message);
        }

        List<Guid> permissionIds = existingPermissions
            .Select(p => p.Id)
            .ToList();

        List<Guid> alreadyLinkedPermissionIds = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        HashSet<Guid> alreadyLinked = new HashSet<Guid>(alreadyLinkedPermissionIds);

        List<RolePermission> toAdd = new List<RolePermission>();
        foreach (Permission p in existingPermissions)
        {
            if (alreadyLinked.Contains(p.Id))
            {
                continue;
            }

            RolePermission link = new RolePermission
            {
                RoleId = roleId,
                PermissionId = p.Id
            };
            toAdd.Add(link);
        }

        if (toAdd.Count == 0)
        {
            return;
        }

        await _db.RolePermissions.AddRangeAsync(toAdd, cancellationToken).ConfigureAwait(false);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}