using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using StockMarketAssistant.AnalyticsService.WebApi;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Api
{
    /// <summary>
    /// Фабрика для создания тестового веб-приложения
    /// </summary>
    public class AnalyticsServiceWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;

        public AnalyticsServiceWebApplicationFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Удаляем существующий DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AnalyticsDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Добавляем тестовый DbContext
                services.AddDbContext<AnalyticsDbContext>(options =>
                {
                    options.UseNpgsql(_connectionString);
                    options.UseSnakeCaseNamingConvention();
                });

                // Применяем миграции
                using var scope = services.BuildServiceProvider().CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
                context.Database.Migrate();
            });

            builder.UseEnvironment("Testing");
        }
    }
}

