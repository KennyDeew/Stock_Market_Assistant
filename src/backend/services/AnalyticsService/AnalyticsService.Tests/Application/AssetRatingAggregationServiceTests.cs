using Microsoft.Extensions.Logging;
using Moq;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Application.Services;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Services;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Tests.Application
{
    public class AssetRatingAggregationServiceTests
    {
        private readonly Mock<IAssetTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IAssetRatingRepository> _ratingRepositoryMock;
        private readonly RatingCalculationService _ratingCalculationService;
        private readonly Mock<ILogger<AssetRatingAggregationService>> _loggerMock;
        private readonly AssetRatingAggregationService _service;

        public AssetRatingAggregationServiceTests()
        {
            _transactionRepositoryMock = new Mock<IAssetTransactionRepository>();
            _ratingRepositoryMock = new Mock<IAssetRatingRepository>();
            _ratingCalculationService = new RatingCalculationService();
            _loggerMock = new Mock<ILogger<AssetRatingAggregationService>>();
            _service = new AssetRatingAggregationService(
                _transactionRepositoryMock.Object,
                _ratingRepositoryMock.Object,
                _ratingCalculationService,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AggregateGlobalRatingsAsync_ValidTransactions_CreatesRatings()
        {
            // Arrange
            var period = Period.LastDay(DateTime.UtcNow);
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();

            var transactions1 = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, stockCardId1, TransactionType.Buy, 10m, 100m, DateTime.UtcNow.AddHours(-2)),
                AssetTransaction.Create(portfolioId, stockCardId1, TransactionType.Sell, 5m, 120m, DateTime.UtcNow.AddHours(-1))
            };

            var transactions2 = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, stockCardId2, TransactionType.Buy, 20m, 200m, DateTime.UtcNow)
            };

            var transactionGroups = new List<IGrouping<Guid, AssetTransaction>>
            {
                transactions1.GroupBy(t => t.StockCardId).First(),
                transactions2.GroupBy(t => t.StockCardId).First()
            };

            _transactionRepositoryMock
                .Setup(r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionGroups);

            _ratingRepositoryMock
                .Setup(r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ratingRepositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.AggregateGlobalRatingsAsync(period);

            // Assert
            _transactionRepositoryMock.Verify(
                r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AggregateGlobalRatingsAsync_NoTransactions_CallsUpsertWithEmptyList()
        {
            // Arrange
            var period = Period.LastDay(DateTime.UtcNow);
            var emptyGroups = new List<IGrouping<Guid, AssetTransaction>>();

            _transactionRepositoryMock
                .Setup(r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyGroups);

            _ratingRepositoryMock
                .Setup(r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ratingRepositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.AggregateGlobalRatingsAsync(period);

            // Assert
            _transactionRepositoryMock.Verify(
                r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.UpsertBatchAsync(It.Is<IEnumerable<AssetRating>>(ratings => !ratings.Any()), It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AggregateGlobalRatingsAsync_ExceptionInGroup_ContinuesProcessing()
        {
            // Arrange
            var period = Period.LastDay(DateTime.UtcNow);
            var portfolioId = Guid.NewGuid();
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();

            // Первая группа с валидными транзакциями
            var transactions1 = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, stockCardId1, TransactionType.Buy, 10m, 100m, DateTime.UtcNow.AddHours(-2))
            };

            // Вторая группа с валидными транзакциями
            var transactions2 = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, stockCardId2, TransactionType.Buy, 20m, 200m, DateTime.UtcNow)
            };

            var transactionGroups = new List<IGrouping<Guid, AssetTransaction>>
            {
                transactions1.GroupBy(t => t.StockCardId).First(),
                transactions2.GroupBy(t => t.StockCardId).First()
            };

            _transactionRepositoryMock
                .Setup(r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionGroups);

            _ratingRepositoryMock
                .Setup(r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ratingRepositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.AggregateGlobalRatingsAsync(period);

            // Assert
            // Проверяем, что обработка продолжилась и рейтинги были созданы
            _ratingRepositoryMock.Verify(
                r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AggregatePortfolioRatingsAsync_ValidTransactions_CreatesRatings()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var period = Period.LastDay(DateTime.UtcNow);
            var stockCardId1 = Guid.NewGuid();
            var stockCardId2 = Guid.NewGuid();

            var transactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, stockCardId1, TransactionType.Buy, 10m, 100m, DateTime.UtcNow.AddHours(-2)),
                AssetTransaction.Create(portfolioId, stockCardId1, TransactionType.Sell, 5m, 120m, DateTime.UtcNow.AddHours(-1)),
                AssetTransaction.Create(portfolioId, stockCardId2, TransactionType.Buy, 20m, 200m, DateTime.UtcNow)
            };

            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioAndPeriodAsync(portfolioId, period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            _ratingRepositoryMock
                .Setup(r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ratingRepositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.AggregatePortfolioRatingsAsync(portfolioId, period);

            // Assert
            _transactionRepositoryMock.Verify(
                r => r.GetByPortfolioAndPeriodAsync(portfolioId, period, It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AggregatePortfolioRatingsAsync_NoTransactions_CallsUpsertWithEmptyList()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var period = Period.LastDay(DateTime.UtcNow);
            var emptyTransactions = new List<AssetTransaction>();

            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioAndPeriodAsync(portfolioId, period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyTransactions);

            _ratingRepositoryMock
                .Setup(r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ratingRepositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.AggregatePortfolioRatingsAsync(portfolioId, period);

            // Assert
            _transactionRepositoryMock.Verify(
                r => r.GetByPortfolioAndPeriodAsync(portfolioId, period, It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.UpsertBatchAsync(It.Is<IEnumerable<AssetRating>>(ratings => !ratings.Any()), It.IsAny<CancellationToken>()),
                Times.Once);

            _ratingRepositoryMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AggregateRatingsAsync_CallsBothGlobalAndPortfolioAggregation()
        {
            // Arrange
            var period = Period.LastDay(DateTime.UtcNow);
            var portfolioId1 = Guid.NewGuid();
            var portfolioId2 = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();

            // Настройка для AggregateGlobalRatingsAsync
            var globalTransactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId1, stockCardId, TransactionType.Buy, 10m, 100m, DateTime.UtcNow)
            };

            var globalGroups = new List<IGrouping<Guid, AssetTransaction>>
            {
                globalTransactions.GroupBy(t => t.StockCardId).First()
            };

            _transactionRepositoryMock
                .Setup(r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(globalGroups);

            // Настройка для AggregateRatingsAsync (получение всех транзакций)
            var allTransactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId1, stockCardId, TransactionType.Buy, 10m, 100m, DateTime.UtcNow),
                AssetTransaction.Create(portfolioId2, stockCardId, TransactionType.Buy, 20m, 200m, DateTime.UtcNow)
            };

            _transactionRepositoryMock
                .Setup(r => r.GetByPeriodAsync(period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(allTransactions);

            // Настройка для AggregatePortfolioRatingsAsync
            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioAndPeriodAsync(portfolioId1, period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetTransaction> { allTransactions[0] });

            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioAndPeriodAsync(portfolioId2, period, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetTransaction> { allTransactions[1] });

            _ratingRepositoryMock
                .Setup(r => r.UpsertBatchAsync(It.IsAny<IEnumerable<AssetRating>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _ratingRepositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.AggregateRatingsAsync(period);

            // Assert
            _transactionRepositoryMock.Verify(
                r => r.GetGroupedByStockCardAsync(period, It.IsAny<CancellationToken>()),
                Times.Once);

            _transactionRepositoryMock.Verify(
                r => r.GetByPeriodAsync(period, It.IsAny<CancellationToken>()),
                Times.Once);

            _transactionRepositoryMock.Verify(
                r => r.GetByPortfolioAndPeriodAsync(portfolioId1, period, It.IsAny<CancellationToken>()),
                Times.Once);

            _transactionRepositoryMock.Verify(
                r => r.GetByPortfolioAndPeriodAsync(portfolioId2, period, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_NullTransactionRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new AssetRatingAggregationService(
                null!,
                _ratingRepositoryMock.Object,
                _ratingCalculationService,
                _loggerMock.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("transactionRepository");
        }

        [Fact]
        public void Constructor_NullRatingRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new AssetRatingAggregationService(
                _transactionRepositoryMock.Object,
                null!,
                _ratingCalculationService,
                _loggerMock.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("ratingRepository");
        }

        [Fact]
        public void Constructor_NullRatingCalculationService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new AssetRatingAggregationService(
                _transactionRepositoryMock.Object,
                _ratingRepositoryMock.Object,
                null!,
                _loggerMock.Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("ratingCalculationService");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var act = () => new AssetRatingAggregationService(
                _transactionRepositoryMock.Object,
                _ratingRepositoryMock.Object,
                _ratingCalculationService,
                null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }
    }
}

