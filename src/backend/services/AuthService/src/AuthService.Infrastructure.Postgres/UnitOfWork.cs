using AuthService.Application.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Infrastructure.Postgres;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AccountsWriteDbContext _db;

    public UnitOfWork(AccountsWriteDbContext db) => _db = db;

    public Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellationToken = default)
        => _db.Database.BeginTransactionAsync(cancellationToken);

    public Task SaveChanges(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}