using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Http;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Http
{
    /// <summary>
    /// Интеграционные тесты для PortfolioServiceClient с WireMock
    /// </summary>
    public class PortfolioServiceClientTests : IDisposable
    {
        private readonly WireMockServer _wireMockServer;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<PortfolioServiceClient>> _loggerMock;
        private readonly PortfolioServiceClient _client;

        public PortfolioServiceClientTests()
        {
            _wireMockServer = WireMockServer.Start();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_wireMockServer.Url!)
            };

            _cache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<PortfolioServiceClient>>();
            _client = new PortfolioServiceClient(_httpClient, _cache, _loggerMock.Object);
        }

        [Fact]
        public async Task GetHistoryAsync_ValidRequest_ReturnsHistory()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var portfolioState = new PortfolioStateDto
            {
                Id = portfolioId,
                UserId = Guid.NewGuid(),
                Name = "Test Portfolio",
                Currency = "RUB",
                Assets = new List<PortfolioAssetStateDto>
                {
                    new PortfolioAssetStateDto
                    {
                        Id = Guid.NewGuid(),
                        PortfolioId = portfolioId,
                        StockCardId = Guid.NewGuid(),
                        Ticker = "SBER",
                        Name = "Сбербанк",
                        AssetType = 1,
                        TotalQuantity = 10,
                        AveragePurchasePrice = 100m,
                        Currency = "RUB"
                    }
                }
            };

            var transactions = new List<PortfolioAssetTransactionResponseDto>
            {
                new PortfolioAssetTransactionResponseDto
                {
                    Id = Guid.NewGuid(),
                    PortfolioAssetId = portfolioState.Assets[0].Id,
                    TransactionType = PortfolioAssetTransactionType.Buy,
                    Quantity = 10,
                    PricePerUnit = 100m,
                    Currency = "RUB",
                    TransactionDate = startDate.AddDays(1)
                }
            };

            // Настройка WireMock для получения состояния портфеля
            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(JsonSerializer.Serialize(portfolioState), "application/json"));

            // Настройка WireMock для получения транзакций
            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolio-assets/{portfolioState.Assets[0].Id}/transactions/period")
                    .WithParam("startDate", new RegexMatcher(".*"))
                    .WithParam("endDate", new RegexMatcher(".*"))
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(JsonSerializer.Serialize(transactions), "application/json"));

            // Act
            var result = await _client.GetHistoryAsync(portfolioId, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result!.PortfolioId.Should().Be(portfolioId);
            result.Transactions.Should().HaveCount(1);
            result.Transactions[0].Quantity.Should().Be(10);
            result.Transactions[0].PricePerUnit.Should().Be(100m);
        }

        [Fact]
        public async Task GetHistoryAsync_CachedRequest_ReturnsFromCache()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var portfolioState = new PortfolioStateDto
            {
                Id = portfolioId,
                UserId = Guid.NewGuid(),
                Name = "Test Portfolio",
                Currency = "RUB",
                Assets = new List<PortfolioAssetStateDto>()
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(JsonSerializer.Serialize(portfolioState), "application/json"));

            // Первый запрос
            var firstResult = await _client.GetHistoryAsync(portfolioId, startDate, endDate);

            // Act - второй запрос (должен быть из кэша)
            var secondResult = await _client.GetHistoryAsync(portfolioId, startDate, endDate);

            // Assert
            secondResult.Should().NotBeNull();
            secondResult!.PortfolioId.Should().Be(portfolioId);

            // Проверяем, что второй запрос был из кэша (WireMock должен быть вызван только один раз)
            _wireMockServer.LogEntries.Should().HaveCount(1); // Только один HTTP запрос
        }

        [Fact]
        public async Task GetHistoryAsync_PortfolioNotFound_ReturnsNull()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.NotFound));

            // Act
            var result = await _client.GetHistoryAsync(portfolioId, startDate, endDate);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCurrentStateAsync_ValidRequest_ReturnsState()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var portfolioState = new PortfolioStateDto
            {
                Id = portfolioId,
                UserId = Guid.NewGuid(),
                Name = "Test Portfolio",
                Currency = "RUB",
                Assets = new List<PortfolioAssetStateDto>
                {
                    new PortfolioAssetStateDto
                    {
                        Id = Guid.NewGuid(),
                        PortfolioId = portfolioId,
                        StockCardId = Guid.NewGuid(),
                        Ticker = "GAZP",
                        Name = "Газпром",
                        AssetType = 1,
                        TotalQuantity = 20,
                        AveragePurchasePrice = 200m,
                        Currency = "RUB"
                    }
                }
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(JsonSerializer.Serialize(portfolioState), "application/json"));

            // Act
            var result = await _client.GetCurrentStateAsync(portfolioId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(portfolioId);
            result.Name.Should().Be("Test Portfolio");
            result.Assets.Should().HaveCount(1);
            result.Assets[0].Ticker.Should().Be("GAZP");
        }

        [Fact]
        public async Task GetMultipleStatesAsync_ValidRequest_ReturnsMultipleStates()
        {
            // Arrange
            var portfolioId1 = Guid.NewGuid();
            var portfolioId2 = Guid.NewGuid();

            var state1 = new PortfolioStateDto
            {
                Id = portfolioId1,
                UserId = Guid.NewGuid(),
                Name = "Portfolio 1",
                Currency = "RUB",
                Assets = new List<PortfolioAssetStateDto>()
            };

            var state2 = new PortfolioStateDto
            {
                Id = portfolioId2,
                UserId = Guid.NewGuid(),
                Name = "Portfolio 2",
                Currency = "RUB",
                Assets = new List<PortfolioAssetStateDto>()
            };

            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId1}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(JsonSerializer.Serialize(state1), "application/json"));

            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId2}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithBody(JsonSerializer.Serialize(state2), "application/json"));

            // Act
            var result = await _client.GetMultipleStatesAsync(new[] { portfolioId1, portfolioId2 });

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainKey(portfolioId1);
            result.Should().ContainKey(portfolioId2);
            result[portfolioId1].Name.Should().Be("Portfolio 1");
            result[portfolioId2].Name.Should().Be("Portfolio 2");
        }

        [Fact]
        public async Task GetHistoryAsync_ServiceUnavailable_ThrowsException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            // Настраиваем WireMock для возврата 503
            _wireMockServer
                .Given(Request.Create()
                    .WithPath($"/api/v1/portfolios/{portfolioId}")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.ServiceUnavailable));

            // Act & Assert
            // Клиент должен пробросить исключение при 503
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _client.GetHistoryAsync(portfolioId, startDate, endDate));
        }

        public void Dispose()
        {
            _wireMockServer?.Stop();
            _wireMockServer?.Dispose();
            _httpClient?.Dispose();
            _cache?.Dispose();
        }
    }
}

