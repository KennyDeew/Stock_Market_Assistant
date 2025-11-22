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
    /// Интеграционные тесты для AssetTransactionRepository с реальной PostgreSQL
    /// </summary>
    [Collection("PostgreSQL")]
    public class AssetTransactionRepositoryTests : IClassFixture<PostgreSqlFixture>, IDisposable
    {
        private readonly PostgreSqlFixture _fixture;
        private readonly AnalyticsDbContext _context;
        private readonly AssetTransactionRepository _repository;

        public AssetTransactionRepositoryTests(PostgreSqlFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.CreateDbContext();
            _repository = new AssetTransactionRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ValidTransaction_SavesToDatabase()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transaction = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                TransactionType.Buy,
                10m,
                100m,
                DateTime.UtcNow);

            // Act
            await _repository.AddAsync(transaction);
            await _repository.SaveChangesAsync();

            // Assert
            var saved = await _context.AssetTransactions.FindAsync(transaction.Id);
            saved.Should().NotBeNull();
            saved!.PortfolioId.Should().Be(portfolioId);
            saved.StockCardId.Should().Be(stockCardId);
            saved.TransactionType.Should().Be(TransactionType.Buy);
            saved.Quantity.Should().Be(10);
            saved.PricePerUnit.Should().Be(100m);
            saved.TotalAmount.Should().Be(1000m);

            // Cleanup
            _context.AssetTransactions.Remove(saved);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingTransaction_ReturnsTransaction()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transaction = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                TransactionType.Sell,
                5m,
                150m,
                DateTime.UtcNow);

            _context.AssetTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(transaction.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(transaction.Id);
            result.TransactionType.Should().Be(TransactionType.Sell);
            result.Quantity.Should().Be(5);
            result.PricePerUnit.Should().Be(150m);

            // Cleanup
            _context.AssetTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByPortfolioIdAsync_ExistingTransactions_ReturnsAllTransactions()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();

            var transaction1 = AssetTransaction.Create(
                portfolioId,
                stockCardId1,
                TransactionType.Buy,
                10m,
                100m,
                DateTime.UtcNow.AddHours(-2));

            var transaction2 = AssetTransaction.Create(
                portfolioId,
                stockCardId2,
                TransactionType.Sell,
                5m,
                150m,
                DateTime.UtcNow.AddHours(-1));

            _context.AssetTransactions.AddRange(transaction1, transaction2);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _repository.GetByPortfolioIdAsync(portfolioId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Id == transaction1.Id);
            result.Should().Contain(t => t.Id == transaction2.Id);
            result.Should().BeInDescendingOrder(t => t.TransactionTime);

            // Cleanup
            _context.AssetTransactions.RemoveRange(transaction1, transaction2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByPeriodAsync_TransactionsInPeriod_ReturnsFilteredTransactions()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            var transactionInPeriod = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                TransactionType.Buy,
                10m,
                100m,
                periodStart.AddDays(1));

            var transactionOutOfPeriod = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                TransactionType.Sell,
                5m,
                150m,
                periodStart.AddDays(-1));

            _context.AssetTransactions.AddRange(transactionInPeriod, transactionOutOfPeriod);
            await _context.SaveChangesAsync();

            var period = new Period(periodStart, periodEnd);

            // Act
            var result = (await _repository.GetByPeriodAsync(period)).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(t => t.Id == transactionInPeriod.Id);
            result.Should().NotContain(t => t.Id == transactionOutOfPeriod.Id);

            // Cleanup
            _context.AssetTransactions.RemoveRange(transactionInPeriod, transactionOutOfPeriod);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetGroupedByStockCardAsync_Transactions_ReturnsGroupedResults()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();

            var transaction1 = AssetTransaction.Create(
                portfolioId,
                stockCardId1,
                TransactionType.Buy,
                10m,
                100m,
                DateTime.UtcNow.AddHours(-2));

            var transaction2 = AssetTransaction.Create(
                portfolioId,
                stockCardId1,
                TransactionType.Sell,
                5m,
                120m,
                DateTime.UtcNow.AddHours(-1));

            var transaction3 = AssetTransaction.Create(
                portfolioId,
                stockCardId2,
                TransactionType.Buy,
                20m,
                200m,
                DateTime.UtcNow);

            _context.AssetTransactions.AddRange(transaction1, transaction2, transaction3);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _repository.GetGroupedByStockCardAsync(null)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(g => g.Key == stockCardId1 && g.Count() == 2);
            result.Should().Contain(g => g.Key == stockCardId2 && g.Count() == 1);

            // Cleanup
            _context.AssetTransactions.RemoveRange(transaction1, transaction2, transaction3);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteAsync_ExistingTransaction_RemovesFromDatabase()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transaction = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                TransactionType.Buy,
                10m,
                100m,
                DateTime.UtcNow);

            _context.AssetTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(transaction);
            await _repository.SaveChangesAsync();

            // Assert
            var deleted = await _context.AssetTransactions.FindAsync(transaction.Id);
            deleted.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    /// <summary>
    /// Коллекция для PostgreSQL фикстуры (один контейнер на все тесты в коллекции)
    /// </summary>
    [CollectionDefinition("PostgreSQL")]
    public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
    {
    }
}

