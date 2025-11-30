using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using StockMarketAssistant.PortfolioService.WebApi;
using Xunit;

namespace StockMarketAssistant.PortfolioService.IntegrationTests
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly HttpClient _client;
        protected readonly WebApplicationFactory<Program> _factory;

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
            });

            _client = _factory.CreateClient();
        }
    }
}