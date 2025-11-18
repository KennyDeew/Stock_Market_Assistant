using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context.Configurations
{
    public class PortfolioAssetTransactionConfiguration : IEntityTypeConfiguration<PortfolioAssetTransaction>
    {
        public void Configure(EntityTypeBuilder<PortfolioAssetTransaction> builder)
        {
            builder.Property(t => t.TransactionType)
                .HasConversion<short>() // Сохраняем enum как целое число (1, 2)
                .IsRequired();

            builder.Property(t => t.TransactionDate)
                .IsRequired();

            builder.Property(t => t.Quantity)
                .IsRequired();

            builder.Property(t => t.PricePerUnit)
                .IsRequired();

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
