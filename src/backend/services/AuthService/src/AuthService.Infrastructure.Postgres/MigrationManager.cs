using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;

namespace AuthService.Infrastructure.Postgres;

public static class MigrationManager
{
    public static async Task MigrateDatabaseAsync<TDbContext>(this IHost host, CancellationToken ct = default)
        where TDbContext : DbContext
    {
        await using var scope = host.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var logger = sp.GetRequiredService<ILogger<TDbContext>>();
        var context = sp.GetRequiredService<TDbContext>();
        var configuration = sp.GetService<IConfiguration>();
        var retryPolicy = sp.GetRequiredService<AsyncRetryPolicy>();

        if (configuration?.GetValue<bool?>("Database:MigrateOnStartup") == false)
        {
            logger.LogInformation("⏭️ Авто-миграции отключены (Database:MigrateOnStartup=false).");
            return;
        }

        logger.LogInformation("⏳ Проверка/применение миграций для {DbContext}…", typeof(TDbContext).Name);

        await retryPolicy.ExecuteAsync(
            async token =>
        {
            await using var _ = await AcquireAdvisoryLockAsync(context.Database.GetDbConnection(), logger, token);

            var pending = (await context.Database.GetPendingMigrationsAsync(token)).ToArray();
            if (pending.Length == 0)
            {
                logger.LogInformation("✅ Миграции уже применены.");
                return;
            }

            logger.LogInformation("📦 Неприменённые миграции ({Count}):", pending.Length);
            foreach (var m in pending) logger.LogInformation("   ➡️ {Migration}", m);

            await context.Database.MigrateAsync(token);
            logger.LogInformation("✅ Миграции применены для {DbContext}.", typeof(TDbContext).Name);
        }, ct);
    }

    // --------- pg_advisory_lock с авто-unlock и закрытием соединения ---------

    private static async Task<IAsyncDisposable> AcquireAdvisoryLockAsync(
        DbConnection connection, ILogger logger, CancellationToken ct)
    {
        if (connection is not NpgsqlConnection npg)
            throw new InvalidOperationException("Для advisory lock требуется NpgsqlConnection.");

        var shouldClose = false;
        if (npg.State != System.Data.ConnectionState.Open)
        {
            await npg.OpenAsync(ct);
            shouldClose = true;
        }

        const long lockKey = 0xAACC_0001L; // стабильная константа для этого сервиса/схемы

        await using (var cmd = new NpgsqlCommand("SELECT pg_advisory_lock(@key);", npg))
        {
            cmd.Parameters.AddWithValue("key", lockKey);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        logger.LogInformation("🔒 Получена pg_advisory_lock (key={Key}).", lockKey);
        return new AsyncRelease(npg, lockKey, shouldClose, logger);
    }

    private sealed class AsyncRelease : IAsyncDisposable
    {
        private readonly NpgsqlConnection _conn;
        private readonly long _key;
        private readonly bool _shouldClose;
        private readonly ILogger _logger;

        public AsyncRelease(NpgsqlConnection conn, long key, bool shouldClose, ILogger logger)
        {
            _conn = conn;
            _key = key;
            _shouldClose = shouldClose;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await using var cmd = new NpgsqlCommand("SELECT pg_advisory_unlock(@key);", _conn);
                cmd.Parameters.AddWithValue("key", _key);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("🔓 Снята pg_advisory_lock (key={Key}).", _key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Не удалось корректно снять pg_advisory_lock (key={Key}).", _key);
            }
            finally
            {
                if (_shouldClose && _conn.State == System.Data.ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
    }
}