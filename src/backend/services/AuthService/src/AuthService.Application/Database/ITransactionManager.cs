using System.Data;

namespace AuthService.Application.Database;

public interface ITransactionManager
{
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}