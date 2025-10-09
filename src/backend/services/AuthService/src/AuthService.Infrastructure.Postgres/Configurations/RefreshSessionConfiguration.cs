using AuthService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

public sealed class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> b)
    {
        b.ToTable("refresh_sessions");
        b.HasKey(x => x.Id);

        b.Property(x => x.CreatedAt).HasColumnType("timestamptz");
        b.Property(x => x.ExpiresIn).HasColumnType("timestamptz");

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.ExpiresIn);
        b.HasIndex(x => x.Jti).IsUnique();
        b.HasIndex(x => x.RefreshToken).IsUnique();

        b.ToTable(tb => tb.HasCheckConstraint(
            "ck_refresh_sessions_expires_after_created",
            "\"expires_in\" > \"created_at\""));

        b.HasIndex(x => new { x.UserId, x.ExpiresIn })
            .HasDatabaseName("ix_refresh_sessions_active");
    }
}