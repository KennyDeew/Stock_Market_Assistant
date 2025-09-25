using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Application.Database;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken);

    Task SaveChanges(CancellationToken cancellationToken);

    IExecutionStrategy CreateExecutionStrategy();
}