using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AuthService.Domain.Entities;

namespace StockMarketAssistant.AuthService.Infrastructure.EntityFramework.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.Name).IsUnique();
        }
    }
}
