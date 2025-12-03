using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context
{
    /// <summary>
    /// Контекст БД
    /// </summary>
    public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
    {
        private static bool _hasWarnedAboutInMemory = false;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Проверяет тип провайдера базы данных при первом использовании.
        /// Выдаёт предупреждение, если используется InMemory в production окружении.
        /// </summary>
        public override int SaveChanges()
        {
            CheckDatabaseProvider();
            return base.SaveChanges();
        }

        /// <summary>
        /// Проверяет тип провайдера базы данных при первом использовании.
        /// Выдаёт предупреждение, если используется InMemory в production окружении.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            CheckDatabaseProvider();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Проверяет, используется ли InMemory провайдер в production окружении.
        /// </summary>
        private void CheckDatabaseProvider()
        {
            if (_hasWarnedAboutInMemory)
            {
                return;
            }

            var isIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "1";
            if (isIntegrationTests)
            {
                return;
            }

            try
            {
                var providerName = Database.ProviderName;
                if (providerName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var loggerFactory = Database.GetService<ILoggerFactory>();
                    var logger = loggerFactory?.CreateLogger<DatabaseContext>();
                    logger?.LogWarning(
                        "ВНИМАНИЕ: Используется InMemory база данных в production окружении. " +
                        "Данные не будут сохраняться между перезапусками приложения. " +
                        "Необходимо настроить строку подключения 'portfolio-db' для подключения к PostgreSQL.");
                    _hasWarnedAboutInMemory = true;
                }
            }
            catch
            {
                // Игнорируем ошибки при проверке провайдера
            }
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        //}
    }
}
