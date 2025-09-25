using AuthService.Contracts;
using AuthService.Infrastructure.Postgres.IdentityManagers;

namespace AuthService.Presentation;

public class AccountsContract : IAccountsContract
{
    private readonly PermissionManager _permissionManager;

    public AccountsContract(PermissionManager permissionManager)
    {
        _permissionManager = permissionManager;
    }

    public async Task<HashSet<string>> GetUserPermissionCodes(Guid userId, CancellationToken ct)
    {
        return await _permissionManager.GetUserPermissionCodes(userId, ct);
    }
}