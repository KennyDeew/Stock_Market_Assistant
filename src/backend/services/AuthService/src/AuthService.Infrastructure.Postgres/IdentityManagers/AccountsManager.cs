using AuthService.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AuthService.Infrastructure.Postgres.IdentityManagers;

public sealed class AccountsManager
{
    private readonly AccountsWriteDbContext _db;
    private readonly ILogger<AccountsManager> _logger;

    public AccountsManager(AccountsWriteDbContext db, ILogger<AccountsManager> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Создаёт AdminAccount для указанного пользователя, если ещё не существует.
    /// Идемпотентно: при повторном вызове вернёт Failure с сообщением без исключений/ошибок EF в логах.
    /// </summary>
    public async Task<Result> CreateAdminAccount(AdminAccount adminAccount, CancellationToken cancellationToken)
    {
        if (adminAccount is null)
            return Result.Failure("AdminAccount is null.");

        // Получаем корректный userId: при новом объекте навигация User есть, а UserId ещё Guid.Empty
        var userId =
            adminAccount.UserId != Guid.Empty
                ? adminAccount.UserId
                : adminAccount.User?.Id ?? Guid.Empty;

        if (userId == Guid.Empty)
            return Result.Failure("AdminAccount must contain a valid User/UserId.");

        // Идемпотентная проверка по фактическому userId
        var exists = await _db.AdminAccounts
            .AsNoTracking()
            .AnyAsync(a => a.UserId == userId, cancellationToken);

        if (exists)
        {
            _logger.LogInformation("AdminAccount уже существует. user_id={UserId}.", userId);
            return Result.Failure("AdminAccount already exists.");
        }

        // Гарантируем, что у сущности установлен корректный внешний ключ
        adminAccount.UserId = userId;

        _db.AdminAccounts.Add(adminAccount);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("AdminAccount создан. user_id={UserId}.", userId);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // На случай гонки между несколькими инстансами — сообщаем, что уже существует
            _logger.LogInformation("AdminAccount уже существует (unique constraint). user_id={UserId}.", userId);
            return Result.Failure("AdminAccount already exists.");
        }
    }
}