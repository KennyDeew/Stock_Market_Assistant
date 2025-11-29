using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context.Configurations
{
    /// <summary>
    /// Конфигурация Entity Framework для сущности Alert
    /// </summary>    
    public class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        /// <summary>
        /// Настройка сущности Alert в базе данных
        /// </summary>
        /// <param name="builder">Строитель сущности</param>        
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            // Свойства
            builder.Property(a => a.StockCardId)
                .IsRequired();

            builder.Property(a => a.AssetType)
                .HasConversion<short>() // Сохраняем enum как целое число (1, 2, 3)
                .IsRequired();

            builder.Property(a => a.AssetTicker)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.AssetName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.TargetPrice)
                .IsRequired();

            builder.Property(a => a.AssetCurrency)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(a => a.Condition)
                .HasConversion<short>() // Сохраняем enum как целое число (1, 2)
                .IsRequired();

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.UserEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.IsActive)
                .IsRequired();

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.Property(a => a.UpdatedAt)
                .IsRequired();

            // Индексы для производительности
            builder.HasIndex(a => a.UserId)
                .IsDescending(false)
                .HasDatabaseName("IX_Alerts_UserId");

            builder.HasIndex(a => a.StockCardId)
                .IsDescending(false)
                .HasDatabaseName("IX_Alerts_StockCardId");

            builder.HasIndex(a => new { a.AssetType, a.AssetTicker })
                .IsDescending(false, false)
                .HasDatabaseName("IX_Alerts_AssetType_Ticker");

            builder.HasIndex(a => a.IsActive)
                .IsDescending(false)
                .HasDatabaseName("IX_Alerts_IsActive");

            // Составной индекс для частых запросов
            builder.HasIndex(a => new { a.IsActive, a.AssetType, a.AssetTicker })
                .HasDatabaseName("IX_Alerts_Active_AssetType_Ticker");
        }
    }
}
