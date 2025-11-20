using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context.Configurations
{
    /// <summary>
    /// Конфигурация Entity Framework для сущности транзакций с активами
    /// </summary>
    public class AssetTransactionConfiguration : IEntityTypeConfiguration<AssetTransaction>
    {
        public void Configure(EntityTypeBuilder<AssetTransaction> builder)
        {
            builder.ToTable("asset_transactions", "public");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id").IsRequired();
            builder.Property(x => x.StockCardId).HasColumnName("stock_card_id").IsRequired();
            builder.Property(x => x.AssetType).HasColumnName("asset_type").IsRequired();
            builder.Property(x => x.TransactionType).HasColumnName("transaction_type").IsRequired();
            builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
            builder.Property(x => x.PricePerUnit).HasColumnName("price_per_unit").IsRequired();
            builder.Property(x => x.TotalAmount).HasColumnName("total_amount").IsRequired();
            builder.Property(x => x.TransactionTime).HasColumnName("transaction_time").IsRequired();
            builder.Property(x => x.Currency).HasColumnName("currency").IsRequired();
            builder.Property(x => x.Metadata).HasColumnName("metadata");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            // Индексы для оптимизации запросов
            builder.HasIndex(x => new { x.PortfolioId, x.TransactionTime });
            builder.HasIndex(x => new { x.StockCardId, x.TransactionTime });
            builder.HasIndex(x => new { x.AssetType, x.TransactionTime });
            builder.HasIndex(x => new { x.TransactionType, x.TransactionTime });
            builder.HasIndex(x => x.TransactionTime);

            // Ограничения
            // Quantity - integer, не требует HasPrecision
            builder.Property(x => x.PricePerUnit).HasPrecision(18, 4);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        }
    }
}

