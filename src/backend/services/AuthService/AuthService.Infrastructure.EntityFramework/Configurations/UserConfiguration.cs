using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AuthService.Domain.Entities;

namespace StockMarketAssistant.AuthService.Infrastructure.EntityFramework.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Nickname).IsUnique();
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
            builder.Property(u => u.LastActiveAt).HasDefaultValueSql("now()");
        }
    }
}
