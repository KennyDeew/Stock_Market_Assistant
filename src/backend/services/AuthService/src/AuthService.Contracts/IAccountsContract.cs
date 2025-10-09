namespace AuthService.Contracts;

public interface IAccountsContract
{
    Task<HashSet<string>> GetUserPermissionCodes(Guid userId, CancellationToken ct);
}