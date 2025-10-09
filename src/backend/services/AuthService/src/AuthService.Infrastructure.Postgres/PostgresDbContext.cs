using AuthService.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres;

public sealed class PostgresDbContext : IdentityDbContext<User, Role, Guid>
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
        : base(options)
    {
    }

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();

    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("accounts");
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresDbContext).Assembly);
    }
}