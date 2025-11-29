using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.WebApi;
using Xunit;

namespace StockMarketAssistant.PortfolioService.IntegrationTests
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly HttpClient _client;
        protected readonly WebApplicationFactory<Program> _factory;

        // Общий InMemoryDatabaseRoot для ВСЕХ тестов в сборке
        // Обеспечивает общий пул данных между запросами
        private static readonly InMemoryDatabaseRoot _databaseRoot = new();

        public IntegrationTestBase(WebApplicationFactory<Program> factory)
        {
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "1");

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:Issuer"] = "TestIssuer",
                        ["Jwt:Audience"] = "TestAudience",
                        ["Jwt:Key"] = "TestKeyVeryLongAndSecureEnoughForTests12345"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Удаляем существующую регистрацию DbContext
                    var descriptors = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<DatabaseContext>) ||
                        d.ServiceType == typeof(DatabaseContext)
                    ).ToList();

                    foreach (var descriptor in descriptors)
                        services.Remove(descriptor);

                    // Регистрируем ОБЩУЮ InMemoryDatabase с использованием _databaseRoot
                    services.AddDbContext<DatabaseContext>(options =>
                    {
                        options.UseInMemoryDatabase("IntegrationTestDb", _databaseRoot);
                    });
                });
            });

            _client = _factory.CreateClient();
        }
    }
}