using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence.Configurations
{
    /// <summary>
    /// Конфигурация Entity Framework для сущности рейтинга активов
    /// </summary>
    public class AssetRatingConfiguration : IEntityTypeConfiguration<AssetRating>
    {
        public void Configure(EntityTypeBuilder<AssetRating> builder)
        {
            builder.ToTable("asset_ratings", "public");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.StockCardId).HasColumnName("stock_card_id").IsRequired();
            builder.Property(x => x.AssetType).HasColumnName("asset_type").IsRequired();
            builder.Property(x => x.Ticker).HasColumnName("ticker").IsRequired().HasMaxLength(20);
            builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            builder.Property(x => x.PeriodStart).HasColumnName("period_start").IsRequired();
            builder.Property(x => x.PeriodEnd).HasColumnName("period_end").IsRequired();
            builder.Property(x => x.BuyTransactionCount).HasColumnName("buy_transaction_count").IsRequired();
            builder.Property(x => x.SellTransactionCount).HasColumnName("sell_transaction_count").IsRequired();
            builder.Property(x => x.TotalBuyAmount).HasColumnName("total_buy_amount").IsRequired();
            builder.Property(x => x.TotalSellAmount).HasColumnName("total_sell_amount").IsRequired();
            builder.Property(x => x.TotalBuyQuantity).HasColumnName("total_buy_quantity").IsRequired();
            builder.Property(x => x.TotalSellQuantity).HasColumnName("total_sell_quantity").IsRequired();
            builder.Property(x => x.TransactionCountRank).HasColumnName("transaction_count_rank").IsRequired();
            builder.Property(x => x.TransactionAmountRank).HasColumnName("transaction_amount_rank").IsRequired();
            builder.Property(x => x.LastUpdated).HasColumnName("last_updated").IsRequired();
            builder.Property(x => x.Context).HasColumnName("context").IsRequired();
            builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            // Precision для decimal полей - (18,2) для amounts согласно требованиям
            builder.Property(x => x.TotalBuyAmount).HasPrecision(18, 2);
            builder.Property(x => x.TotalSellAmount).HasPrecision(18, 2);
            // TotalBuyQuantity и TotalSellQuantity - integer, не требуют HasPrecision

            // Индексы для оптимизации запросов
            builder.HasIndex(x => x.StockCardId)
                .HasDatabaseName("ix_asset_ratings_stock_card_id");

            builder.HasIndex(x => new { x.PeriodStart, x.PeriodEnd })
                .HasDatabaseName("ix_asset_ratings_period_start_period_end");

            builder.HasIndex(x => new { x.Context, x.PortfolioId })
                .HasDatabaseName("ix_asset_ratings_context_portfolio_id");

            builder.HasIndex(x => new { x.StockCardId, x.PeriodStart, x.PeriodEnd, x.Context, x.PortfolioId })
                .HasDatabaseName("ix_asset_ratings_stock_card_id_period_start_period_end_context_portfolio_id");

            builder.HasIndex(x => new { x.AssetType, x.PeriodStart, x.PeriodEnd, x.Context })
                .HasDatabaseName("ix_asset_ratings_asset_type_period_start_period_end_context");

            builder.HasIndex(x => new { x.Context, x.PortfolioId, x.PeriodStart, x.PeriodEnd })
                .HasDatabaseName("ix_asset_ratings_context_portfolio_id_period_start_period_end");

            builder.HasIndex(x => new { x.TransactionCountRank, x.Context, x.PeriodStart, x.PeriodEnd })
                .HasDatabaseName("ix_asset_ratings_transaction_count_rank_context_period_start_period_end");

            builder.HasIndex(x => new { x.TransactionAmountRank, x.Context, x.PeriodStart, x.PeriodEnd })
                .HasDatabaseName("ix_asset_ratings_transaction_amount_rank_context_period_start_period_end");

            // Unique constraint: (StockCardId, PeriodStart, PeriodEnd, Context) WHERE portfolio_id IS NULL для Global контекста
            // Для Portfolio контекста уникальность по (StockCardId, PeriodStart, PeriodEnd, Context, PortfolioId)
            // Это сложное ограничение, которое лучше реализовать через миграцию или частичный индекс
        }
    }
}

