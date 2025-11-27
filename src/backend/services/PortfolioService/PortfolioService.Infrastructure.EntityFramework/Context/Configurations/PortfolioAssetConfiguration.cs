using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context.Configurations
{
    /// <summary>
    /// Конфигурация Entity Framework для сущности PortfolioAsset
    /// </summary>    
    public class PortfolioAssetConfiguration : IEntityTypeConfiguration<PortfolioAsset>
    {
        /// <summary>
        /// Настройка сущности PortfolioAsset в базе данных
        /// </summary>
        public void Configure(EntityTypeBuilder<PortfolioAsset> builder)
        {
            builder.Property(a => a.AssetType)
                .HasConversion<short>() // Сохраняем enum как целое число (1, 2, 3)
                .IsRequired();

            builder.HasMany(a => a.Transactions)
                .WithOne(t => t.PortfolioAsset)
                .HasForeignKey(t => t.PortfolioAssetId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.StockCardId)
                .IsRequired();
        }
    }
}
