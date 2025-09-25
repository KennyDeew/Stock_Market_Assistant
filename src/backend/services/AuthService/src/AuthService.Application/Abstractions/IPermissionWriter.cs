namespace AuthService.Application.Abstractions;

public interface IPermissionWriter
{
    Task AddRangeIfNotExists(IEnumerable<string> permissionCodes, CancellationToken cancellationToken);
}