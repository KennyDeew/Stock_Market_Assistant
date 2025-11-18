using AuthService.Domain;

namespace AuthService.Application.Abstractions;

public interface IPermissionReader
{
    Task<Permission?> FindByCode(string code, CancellationToken cancellationToken);

    Task<HashSet<string>> GetUserPermissionCodes(Guid userId, CancellationToken cancellationToken);
}