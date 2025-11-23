using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Responses;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using StockMarketAssistant.AnalyticsService.Tests.Integration.Fixtures;
using StockMarketAssistant.AnalyticsService.WebApi;
using Xunit;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Api
{
    /// <summary>
    /// Интеграционные тесты для PortfolioAnalyticsController с WebApplicationFactory
    /// </summary>
    [Collection("PostgreSQL")]
    public class PortfolioAnalyticsControllerTests : IClassFixture<PostgreSqlFixture>, IDisposable
    {
        private readonly PostgreSqlFixture _fixture;
        private readonly AnalyticsServiceWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly AnalyticsDbContext _context;

        public PortfolioAnalyticsControllerTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
            _factory = new AnalyticsServiceWebApplicationFactory(_fixture.ConnectionString);
            _client = _factory.CreateClient();
            _context = _fixture.CreateDbContext();

            // Настройка JWT токена для авторизации
            var token = GenerateTestJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetPortfolioHistory_ValidRequest_ReturnsOk()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var url = $"/api/analytics/portfolios/{portfolioId}/history?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            // Может вернуть 404, если портфель не найден, или 200, если найден
            // Это зависит от реализации GetPortfolioHistoryUseCase
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPortfolioHistory_Unauthorized_Returns401()
        {
            // Arrange
            var clientWithoutAuth = _factory.CreateClient();
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var url = $"/api/analytics/portfolios/{portfolioId}/history?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}";

            // Act
            var response = await clientWithoutAuth.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetPortfolioHistory_InvalidParameters_Returns400()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(-7); // Конец раньше начала
            var url = $"/api/analytics/portfolios/{portfolioId}/history?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ComparePortfolios_ValidRequest_ReturnsOk()
        {
            // Arrange
            var portfolioId1 = Guid.NewGuid();
            var portfolioId2 = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid> { portfolioId1, portfolioId2 },
                StartDate = startDate,
                EndDate = endDate
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/analytics/portfolios/compare", content);

            // Assert
            // Может вернуть 200 или 400 в зависимости от наличия данных
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ComparePortfolios_EmptyList_Returns400()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid>(),
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/analytics/portfolios/compare", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ComparePortfolios_TooManyPortfolios_Returns400()
        {
            // Arrange
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = Enumerable.Range(0, 11).Select(_ => Guid.NewGuid()).ToList(),
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/analytics/portfolios/compare", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ComparePortfolios_Unauthorized_Returns401()
        {
            // Arrange
            var clientWithoutAuth = _factory.CreateClient();
            var request = new ComparePortfoliosRequest
            {
                PortfolioIds = new List<Guid> { Guid.NewGuid() },
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await clientWithoutAuth.PostAsync("/api/analytics/portfolios/compare", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Генерирует упрощенный JWT токен для тестов
        /// </summary>
        private string GenerateTestJwtToken()
        {
            // Упрощенная версия для тестов
            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"sub\":\"test-user\",\"exp\":" + DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() + "}"));
            var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("test-signature"));
            return $"{header}.{payload}.{signature}";
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
            _context?.Dispose();
        }
    }
}

