using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AuthService.Infrastructure.Postgres.IdentityManagers;

public class AccountsManager
{
    private readonly AccountsWriteDbContext _context;

    public AccountsManager(AccountsWriteDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<Guid, Error>> CreateAdminAccount(
        AdminAccount adminAccount,
        CancellationToken cancellationToken = default)
    {
        if (adminAccount is null)
            return Errors.General.ValueIsInvalid("AdminAccount");

        await _context.AdminAccounts.AddAsync(adminAccount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return adminAccount.Id;
    }
}