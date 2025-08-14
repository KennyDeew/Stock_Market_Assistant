using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context.Configurations;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context
{
    /// <summary>
    /// Контекст базы данных для аналитического сервиса
    /// </summary>
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Транзакции с активами
        /// </summary>
        public DbSet<AssetTransaction> AssetTransactions { get; set; }

        /// <summary>
        /// Рейтинги активов
        /// </summary>
        public DbSet<AssetRating> AssetRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Применение конфигураций
            modelBuilder.ApplyConfiguration(new AssetTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new AssetRatingConfiguration());

            // Схема по умолчанию - public (не указываем явно)
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("analytics-db");
            }
        }
    }
}
