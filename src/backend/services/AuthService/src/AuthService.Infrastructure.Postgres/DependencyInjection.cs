using AuthService.Application.Abstractions;
using AuthService.Application.Database;
using AuthService.Application.JWT;
using AuthService.Domain;
using AuthService.Infrastructure.Postgres.IdentityManagers;
using AuthService.Infrastructure.Postgres.Options;
using AuthService.Infrastructure.Postgres.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;

namespace AuthService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ITokenProvider, JwtTokenProvider>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.SectionName));

        services.RegisterIdentity();

        services.AddSingleton<AccountsSeeder>();
        services.AddScoped<AccountsSeederService>();

        return services;
    }

    private static void RegisterIdentity(this IServiceCollection services)
    {
        services
            .AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AccountsWriteDbContext>();

        services.AddScoped<RolePermissionManager>();
        services.AddScoped<IRefreshSessionManager, RefreshSessionManager>();
    }

    public static IServiceCollection AddPostgresInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<AccountsWriteDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("Database"), npg =>
            {
                npg.EnableRetryOnFailure();
                npg.MigrationsHistoryTable("__EFMigrationsHistory", "accounts");
            });

            opt.UseSnakeCaseNamingConvention();

#if DEBUG
            opt.EnableDetailedErrors();
            opt.EnableSensitiveDataLogging();
#endif
        });

        // менеджеры прав и аккаунтов
        services.AddScoped<IPermissionReader, PermissionManager>();
        services.AddScoped<IPermissionWriter, PermissionManager>();
        services.AddScoped<PermissionManager>();
        services.AddScoped<AccountsManager>();

        // ✅ UoW (на IDbContextTransaction)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Регистрирует политику ретраев для миграций:
    /// 5 попыток, экспоненциальная пауза 2s, 4s, 8s, 16s, 32s.
    /// Ловим временные ошибки PostgreSQL/ADO.
    /// </summary>
    public static IServiceCollection AddMigrationResilience(this IServiceCollection services)
    {
        services.AddSingleton<AsyncRetryPolicy>(sp =>
            Policy
                .Handle<NpgsqlException>()
                .Or<DbException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, delay, attempt, _) =>
                    {
                        var logger = sp.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("MigrationRetry");
                        logger.LogWarning(ex,
                            "🔁 [{Attempt}/5] Повтор через {Delay}. Причина: {Message}",
                            attempt, delay, ex.Message);
                    }));

        return services;
    }
}
