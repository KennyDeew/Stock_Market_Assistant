using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context.Configurations
{
    /// <summary>
    /// Конфигурация Entity Framework для сущности OutboxMessage
    /// </summary>    
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        /// <summary>
        /// Настройка сущности OutboxMessage в базе данных
        /// </summary>
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.Property(x => x.Topic)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Message)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt)
                .IsRequired(false);

            builder.Property(x => x.Error)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(x => x.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            // Индексы для производительности
            builder.HasIndex(x => new { x.ProcessedAt, x.CreatedAt })
                .HasDatabaseName("IX_OutboxMessages_Processed_Created");

            builder.HasIndex(x => x.Topic)
                .HasDatabaseName("IX_OutboxMessages_Topic");
        }
    }
}