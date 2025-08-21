using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Application.Database;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default);

    Task SaveChanges(CancellationToken cancellationToken = default);
}