using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AuthService.Infrastructure.Postgres.Seeding;

public class AccountsSeeder
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AccountsSeeder(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AccountsSeeder>>();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        var svc = scope.ServiceProvider.GetRequiredService<AccountsSeederService>();

        const long LockKey = 0x0ACC_0000_0000_0001L;

        var connectionString = db.Database.GetConnectionString()
                               ?? db.Database.GetDbConnection().ConnectionString;

        await using var conn = new NpgsqlConnection(connectionString);

        try
        {
            await conn.OpenAsync(ct);

            await using (var lockCmd = new NpgsqlCommand("select pg_try_advisory_lock(@k);", conn))
            {
                lockCmd.Parameters.AddWithValue("k", LockKey);
                var gotLock = (bool)(await lockCmd.ExecuteScalarAsync(ct))!;
                if (!gotLock)
                {
                    logger.LogInformation(
                        "Пропускаю сидинг: другой инстанс держит advisory lock {LockKey}.",
                        LockKey);
                    return;
                }
            }

            try
            {
                await svc.SeedAsync(ct);
                logger.LogInformation("Accounts seeding завершён успешно.");
            }
            finally
            {
                await using var unlockCmd = new NpgsqlCommand("select pg_advisory_unlock(@k);", conn);
                unlockCmd.Parameters.AddWithValue("k", LockKey);
                await unlockCmd.ExecuteNonQueryAsync(ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Accounts seeding завершился ошибкой.");
        }
    }
}