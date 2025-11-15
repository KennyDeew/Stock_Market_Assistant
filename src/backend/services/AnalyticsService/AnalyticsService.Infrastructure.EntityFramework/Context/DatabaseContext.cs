using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context
{
    /// <summary>
    /// Контекст базы данных для аналитического сервиса
    /// </summary>
    public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
    {
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

            // Применение конфигураций из сборки
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        }
    }
}



