using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AuthService.Domain.Entities;

namespace StockMarketAssistant.AuthService.Infrastructure.EntityFramework.Configurations
{
    public class UserCredentialsLocalConfiguration : IEntityTypeConfiguration<UserCredentialsLocal>
    {
        public void Configure(EntityTypeBuilder<UserCredentialsLocal> builder)
        {
            builder.HasKey(c => c.UserId);

            builder.HasOne(c => c.User)
                .WithOne(u => u.Credentials)
                .HasForeignKey<UserCredentialsLocal>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.Email).IsUnique();
            builder.HasIndex(c => c.Nickname).IsUnique();
            builder.Property(c => c.LastPasswordChange).HasDefaultValueSql("now()");
        }
    }
}
