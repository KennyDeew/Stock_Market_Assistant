using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Responses;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using StockMarketAssistant.AnalyticsService.Tests.Integration.Fixtures;
using StockMarketAssistant.AnalyticsService.WebApi;
using Xunit;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Api
{
    /// <summary>
    /// Интеграционные тесты для AssetAnalyticsController с WebApplicationFactory
    /// </summary>
    [Collection("PostgreSQL")]
    public class AssetAnalyticsControllerTests : IClassFixture<PostgreSqlFixture>, IDisposable
    {
        private readonly PostgreSqlFixture _fixture;
        private readonly AnalyticsServiceWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly AnalyticsDbContext _context;

        public AssetAnalyticsControllerTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
            _factory = new AnalyticsServiceWebApplicationFactory(_fixture.ConnectionString);
            _client = _factory.CreateClient();
            _context = _fixture.CreateDbContext();

            // Настройка JWT токена для авторизации (упрощенная версия для тестов)
            var token = GenerateTestJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetTopBoughtAssets_ValidRequest_ReturnsOk()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Создаем тестовые данные
            var transaction1 = AssetTransaction.Create(
                portfolioId,
                stockCardId1,
                TransactionType.Buy,
                15m,
                100m,
                periodStart.AddDays(1));

            var transaction2 = AssetTransaction.Create(
                portfolioId,
                stockCardId2,
                TransactionType.Buy,
                10m,
                200m,
                periodStart.AddDays(2));

            _context.AssetTransactions.AddRange(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Создаем рейтинги
            var rating1 = AssetRating.CreateGlobalRating(
                stockCardId1,
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating1.UpdateStatistics(15, 0, 1500m, 0m, 15, 0, 1, 1);

            var rating2 = AssetRating.CreateGlobalRating(
                stockCardId2,
                AssetType.Share,
                "GAZP",
                "Газпром",
                periodStart,
                periodEnd);

            rating2.UpdateStatistics(10, 0, 2000m, 0m, 10, 0, 2, 1);

            _context.AssetRatings.AddRange(rating1, rating2);
            await _context.SaveChangesAsync();

            var url = $"/api/analytics/assets/top-bought?StartDate={periodStart:yyyy-MM-ddTHH:mm:ssZ}&EndDate={periodEnd:yyyy-MM-ddTHH:mm:ssZ}&Top=2&Context=1";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopAssetsResponseDto>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.Assets.Should().HaveCount(2);
            result.Assets[0].BuyTransactionCount.Should().BeGreaterThanOrEqualTo(result.Assets[1].BuyTransactionCount);

            // Cleanup
            _context.AssetRatings.RemoveRange(rating1, rating2);
            _context.AssetTransactions.RemoveRange(transaction1, transaction2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetTopBoughtAssets_Unauthorized_Returns401()
        {
            // Arrange
            var clientWithoutAuth = _factory.CreateClient();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;
            var url = $"/api/analytics/assets/top-bought?StartDate={periodStart:yyyy-MM-ddTHH:mm:ssZ}&EndDate={periodEnd:yyyy-MM-ddTHH:mm:ssZ}&Top=10&Context=1";

            // Act
            var response = await clientWithoutAuth.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetTopBoughtAssets_InvalidParameters_Returns400()
        {
            // Arrange
            var periodStart = DateTime.UtcNow;
            var periodEnd = DateTime.UtcNow.AddDays(-7); // Конец раньше начала
            var url = $"/api/analytics/assets/top-bought?StartDate={periodStart:yyyy-MM-ddTHH:mm:ssZ}&EndDate={periodEnd:yyyy-MM-ddTHH:mm:ssZ}&Top=10&Context=1";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetTopSoldAssets_ValidRequest_ReturnsOk()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Создаем тестовые данные
            var transaction1 = AssetTransaction.Create(
                portfolioId,
                stockCardId1,
                TransactionType.Sell,
                8m,
                150m,
                periodStart.AddDays(1));

            var transaction2 = AssetTransaction.Create(
                portfolioId,
                stockCardId2,
                TransactionType.Sell,
                5m,
                200m,
                periodStart.AddDays(2));

            _context.AssetTransactions.AddRange(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Создаем рейтинги
            var rating1 = AssetRating.CreateGlobalRating(
                stockCardId1,
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating1.UpdateStatistics(0, 8, 0m, 1200m, 0, 8, 1, 1);

            var rating2 = AssetRating.CreateGlobalRating(
                stockCardId2,
                AssetType.Share,
                "GAZP",
                "Газпром",
                periodStart,
                periodEnd);

            rating2.UpdateStatistics(0, 5, 0m, 1000m, 0, 5, 2, 2);

            _context.AssetRatings.AddRange(rating1, rating2);
            await _context.SaveChangesAsync();

            var url = $"/api/analytics/assets/top-sold?StartDate={periodStart:yyyy-MM-ddTHH:mm:ssZ}&EndDate={periodEnd:yyyy-MM-ddTHH:mm:ssZ}&Top=2&Context=1";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopAssetsResponseDto>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            result.Should().NotBeNull();
            result!.Assets.Should().HaveCount(2);
            result.Assets[0].SellTransactionCount.Should().BeGreaterThanOrEqualTo(result.Assets[1].SellTransactionCount);

            // Cleanup
            _context.AssetRatings.RemoveRange(rating1, rating2);
            _context.AssetTransactions.RemoveRange(transaction1, transaction2);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Генерирует упрощенный JWT токен для тестов
        /// </summary>
        private string GenerateTestJwtToken()
        {
            // Упрощенная версия для тестов
            // В реальном приложении используется полноценный JWT токен
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

