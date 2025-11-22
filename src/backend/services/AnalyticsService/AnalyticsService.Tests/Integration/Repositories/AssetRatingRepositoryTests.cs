using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence.Repositories;
using StockMarketAssistant.AnalyticsService.Tests.Integration.Fixtures;
using Xunit;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Repositories
{
    /// <summary>
    /// Интеграционные тесты для AssetRatingRepository с реальной PostgreSQL
    /// </summary>
    [Collection("PostgreSQL")]
    public class AssetRatingRepositoryTests : IClassFixture<PostgreSqlFixture>, IDisposable
    {
        private readonly PostgreSqlFixture _fixture;
        private readonly AnalyticsDbContext _context;
        private readonly AssetRatingRepository _repository;

        public AssetRatingRepositoryTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.CreateDbContext();
            _repository = new AssetRatingRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ValidRating_SavesToDatabase()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating.UpdateStatistics(10, 5, 10000m, 5000m, 100, 50, 1, 2);

            // Act
            await _repository.AddAsync(rating);
            await _repository.SaveChangesAsync();

            // Assert
            var saved = await _context.AssetRatings.FindAsync(rating.Id);
            saved.Should().NotBeNull();
            saved!.StockCardId.Should().Be(stockCardId);
            saved.PortfolioId.Should().Be(portfolioId);
            saved.Context.Should().Be(AnalysisContext.Portfolio);
            saved.BuyTransactionCount.Should().Be(10);
            saved.SellTransactionCount.Should().Be(5);

            // Cleanup
            _context.AssetRatings.Remove(saved);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByStockCardAndPeriodAsync_ExistingRating_ReturnsRating()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                AssetType.Share,
                "GAZP",
                "Газпром",
                periodStart,
                periodEnd);

            rating.UpdateStatistics(8, 4, 8000m, 4000m, 80, 40, 1, 1);

            _context.AssetRatings.Add(rating);
            await _context.SaveChangesAsync();

            var period = new Period(periodStart, periodEnd);

            // Act
            var result = await _repository.GetByStockCardAndPeriodAsync(
                stockCardId,
                period,
                AnalysisContext.Portfolio,
                portfolioId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(rating.Id);
            result.StockCardId.Should().Be(stockCardId);
            result.PortfolioId.Should().Be(portfolioId);

            // Cleanup
            _context.AssetRatings.Remove(rating);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task UpsertAsync_NewRating_CreatesRating()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                AssetType.Share,
                "LKOH",
                "Лукойл",
                periodStart,
                periodEnd);

            rating.UpdateStatistics(5, 3, 5000m, 3000m, 50, 30, 1, 1);

            // Act
            await _repository.UpsertAsync(rating);
            await _repository.SaveChangesAsync();

            // Assert
            var saved = await _context.AssetRatings.FindAsync(rating.Id);
            saved.Should().NotBeNull();
            saved!.Ticker.Should().Be("LKOH");

            // Cleanup
            _context.AssetRatings.Remove(saved);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task UpsertAsync_ExistingRating_UpdatesRating()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating.UpdateStatistics(10, 5, 10000m, 5000m, 100, 50, 1, 2);

            _context.AssetRatings.Add(rating);
            await _context.SaveChangesAsync();

            // Обновляем рейтинг
            rating.UpdateStatistics(15, 8, 15000m, 8000m, 150, 80, 1, 1);

            // Act
            await _repository.UpsertAsync(rating);
            await _repository.SaveChangesAsync();

            // Assert
            var updated = await _context.AssetRatings.FindAsync(rating.Id);
            updated.Should().NotBeNull();
            updated!.BuyTransactionCount.Should().Be(15);
            updated.SellTransactionCount.Should().Be(8);
            updated.TotalBuyAmount.Should().Be(15000m);

            // Cleanup
            _context.AssetRatings.Remove(updated);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task UpsertBatchAsync_MultipleRatings_UpsertsAllRatings()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating1 = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId1,
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating1.UpdateStatistics(10, 5, 10000m, 5000m, 100, 50, 1, 1);

            var rating2 = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId2,
                AssetType.Share,
                "GAZP",
                "Газпром",
                periodStart,
                periodEnd);

            rating2.UpdateStatistics(8, 4, 8000m, 4000m, 80, 40, 1, 2);

            var ratings = new[] { rating1, rating2 };

            // Act
            await _repository.UpsertBatchAsync(ratings);
            await _repository.SaveChangesAsync();

            // Assert
            var saved1 = await _context.AssetRatings.FindAsync(rating1.Id);
            var saved2 = await _context.AssetRatings.FindAsync(rating2.Id);

            saved1.Should().NotBeNull();
            saved2.Should().NotBeNull();
            saved1!.Ticker.Should().Be("SBER");
            saved2!.Ticker.Should().Be("GAZP");

            // Cleanup
            _context.AssetRatings.RemoveRange(saved1, saved2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetTopBoughtAsync_ExistingRatings_ReturnsTopRatings()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating1 = AssetRating.CreatePortfolioRating(
                portfolioId,
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating1.UpdateStatistics(15, 5, 15000m, 5000m, 150, 50, 1, 1);

            var rating2 = AssetRating.CreatePortfolioRating(
                portfolioId,
                Guid.NewGuid(),
                AssetType.Share,
                "GAZP",
                "Газпром",
                periodStart,
                periodEnd);

            rating2.UpdateStatistics(10, 4, 10000m, 4000m, 100, 40, 1, 2);

            var rating3 = AssetRating.CreatePortfolioRating(
                portfolioId,
                Guid.NewGuid(),
                AssetType.Share,
                "LKOH",
                "Лукойл",
                periodStart,
                periodEnd);

            rating3.UpdateStatistics(5, 3, 5000m, 3000m, 50, 30, 1, 3);

            _context.AssetRatings.AddRange(rating1, rating2, rating3);
            await _context.SaveChangesAsync();

            var period = new Period(periodStart, periodEnd);

            // Act
            var result = (await _repository.GetTopBoughtAsync(
                2,
                period,
                AnalysisContext.Portfolio,
                portfolioId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].BuyTransactionCount.Should().Be(15); // SBER
            result[1].BuyTransactionCount.Should().Be(10); // GAZP

            // Cleanup
            _context.AssetRatings.RemoveRange(rating1, rating2, rating3);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetTopSoldAsync_ExistingRatings_ReturnsTopRatings()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var rating1 = AssetRating.CreatePortfolioRating(
                portfolioId,
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                periodStart,
                periodEnd);

            rating1.UpdateStatistics(10, 8, 10000m, 8000m, 100, 80, 1, 1);

            var rating2 = AssetRating.CreatePortfolioRating(
                portfolioId,
                Guid.NewGuid(),
                AssetType.Share,
                "GAZP",
                "Газпром",
                periodStart,
                periodEnd);

            rating2.UpdateStatistics(10, 5, 10000m, 5000m, 100, 50, 1, 2);

            _context.AssetRatings.AddRange(rating1, rating2);
            await _context.SaveChangesAsync();

            var period = new Period(periodStart, periodEnd);

            // Act
            var result = (await _repository.GetTopSoldAsync(
                2,
                period,
                AnalysisContext.Portfolio,
                portfolioId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].SellTransactionCount.Should().Be(8); // SBER
            result[1].SellTransactionCount.Should().Be(5); // GAZP

            // Cleanup
            _context.AssetRatings.RemoveRange(rating1, rating2);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

