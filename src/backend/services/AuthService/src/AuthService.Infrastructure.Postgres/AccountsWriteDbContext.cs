using AuthService.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres;

public sealed class AccountsWriteDbContext : IdentityDbContext<User, Role, Guid>
{
    public AccountsWriteDbContext(DbContextOptions<AccountsWriteDbContext> options)
        : base(options)
    {
    }

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();

    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1) схема по умолчанию
        modelBuilder.HasDefaultSchema("accounts");

        // 2) базовая конфигурация Identity
        base.OnModelCreating(modelBuilder);

        // 3) расширение PostgreSQL
        modelBuilder.HasPostgresExtension("citext");

        // 4) применяем все IEntityTypeConfiguration<> из этой сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsWriteDbContext).Assembly);
    }
}

