using AuthService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

public sealed class AdminAccountConfiguration : IEntityTypeConfiguration<AdminAccount>
{
    public void Configure(EntityTypeBuilder<AdminAccount> b)
    {
        b.ToTable("admin_accounts");
        b.HasKey(x => x.Id);

        b.HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<AdminAccount>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(a => a.UserId).IsUnique();

        b.ComplexProperty(a => a.FullName, n =>
        {
            n.Property(p => p.FirstName).IsRequired().HasColumnName("first_name");
            n.Property(p => p.SecondName).IsRequired().HasColumnName("second_name");
        });
    }
}