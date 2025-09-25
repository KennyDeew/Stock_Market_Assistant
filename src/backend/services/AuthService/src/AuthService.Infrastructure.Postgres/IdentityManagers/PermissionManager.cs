using AuthService.Application.Abstractions;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres.IdentityManagers;

public class PermissionManager : IPermissionReader, IPermissionWriter
{
    private readonly AccountsWriteDbContext _dbContext;

    public PermissionManager(AccountsWriteDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Permission?> FindByCode(string code, CancellationToken cancellationToken)
        => await _dbContext.Permissions.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

    public async Task AddRangeIfNotExists(IEnumerable<string> permissionCodes, CancellationToken cancellationToken)
    {
        foreach (var code in permissionCodes)
        {
            var exists = await _dbContext.Permissions
                .AnyAsync(p => p.Code == code, cancellationToken);

            if (!exists)
            {
                await _dbContext.Permissions.AddAsync(new Permission { Code = code }, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<HashSet<string>> GetUserPermissionCodes(Guid userId, CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Users
            .Include(u => u.Roles)
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .ToListAsync(cancellationToken);

        return permissions.ToHashSet();
    }
}