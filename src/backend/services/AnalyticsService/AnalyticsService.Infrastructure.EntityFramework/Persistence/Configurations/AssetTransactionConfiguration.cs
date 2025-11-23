using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence.Configurations
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
            builder.Property(x => x.Currency).HasColumnName("currency").IsRequired().HasMaxLength(10);
            builder.Property(x => x.Metadata).HasColumnName("metadata");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            // Precision для decimal полей
            // Quantity - integer в доменной модели, но если бы был decimal, то (18,8)
            // PricePerUnit и TotalAmount - (18,2) согласно требованиям
            builder.Property(x => x.PricePerUnit).HasPrecision(18, 2);
            builder.Property(x => x.TotalAmount).HasPrecision(18, 2);

            // Индексы для оптимизации запросов
            builder.HasIndex(x => x.StockCardId)
                .HasDatabaseName("ix_asset_transactions_stock_card_id");

            builder.HasIndex(x => x.PortfolioId)
                .HasDatabaseName("ix_asset_transactions_portfolio_id");

            builder.HasIndex(x => x.TransactionTime)
                .HasDatabaseName("ix_asset_transactions_transaction_time");

            builder.HasIndex(x => new { x.StockCardId, x.TransactionTime })
                .HasDatabaseName("ix_asset_transactions_stock_card_id_transaction_time");

            builder.HasIndex(x => new { x.PortfolioId, x.TransactionTime })
                .HasDatabaseName("ix_asset_transactions_portfolio_id_transaction_time");

            builder.HasIndex(x => new { x.AssetType, x.TransactionTime })
                .HasDatabaseName("ix_asset_transactions_asset_type_transaction_time");

            builder.HasIndex(x => new { x.TransactionType, x.TransactionTime })
                .HasDatabaseName("ix_asset_transactions_transaction_type_transaction_time");
        }
    }
}

