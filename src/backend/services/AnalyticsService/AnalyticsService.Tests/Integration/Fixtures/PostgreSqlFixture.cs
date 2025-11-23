using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Fixtures
{
    /// <summary>
    /// Фикстура для PostgreSQL контейнера в интеграционных тестах
    /// </summary>
    public class PostgreSqlFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer;
        private string _connectionString = string.Empty;

        public PostgreSqlFixture()
        {
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:17.5")
                .WithDatabase("analytics_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public string ConnectionString => _connectionString;

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
            _connectionString = _postgreSqlContainer.GetConnectionString();

            // Применяем миграции
            var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
                .UseNpgsql(_connectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            using var context = new AnalyticsDbContext(options);
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync();
        }

        /// <summary>
        /// Создать новый контекст базы данных для тестов
        /// </summary>
        public AnalyticsDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
                .UseNpgsql(_connectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            return new AnalyticsDbContext(options);
        }
    }
}

