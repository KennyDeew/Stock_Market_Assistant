using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AuthService.Domain.Entities;

namespace StockMarketAssistant.AuthService.Infrastructure.EntityFramework.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(p => new { p.Action, p.Resource }).IsUnique();
        }
    }
}
