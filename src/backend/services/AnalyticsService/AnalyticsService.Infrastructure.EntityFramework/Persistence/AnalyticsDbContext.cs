using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence
{
    /// <summary>
    /// Контекст базы данных для аналитического сервиса
    /// </summary>
    public class AnalyticsDbContext : DbContext
    {
        /// <summary>
        /// Транзакции с активами
        /// </summary>
        public DbSet<AssetTransaction> AssetTransactions { get; set; }

        /// <summary>
        /// Рейтинги активов
        /// </summary>
        public DbSet<AssetRating> AssetRatings { get; set; }

        /// <summary>
        /// Конструктор для внедрения зависимостей
        /// </summary>
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Настройка модели данных
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Применение конфигураций из сборки
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
        }
    }
}

