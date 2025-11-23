using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Services;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Tests.Domain
{
    public class RatingCalculationServiceTests
    {
        private readonly RatingCalculationService _service;

        public RatingCalculationServiceTests()
        {
            _service = new RatingCalculationService();
        }

        [Fact]
        public void CreateRating_ValidTransactions_CreatesRating()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var period = Period.LastDay(DateTime.UtcNow);
            var transactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, stockCardId, TransactionType.Buy, 10m, 100m, DateTime.UtcNow.AddHours(-2)),
                AssetTransaction.Create(portfolioId, stockCardId, TransactionType.Buy, 5m, 110m, DateTime.UtcNow.AddHours(-1)),
                AssetTransaction.Create(portfolioId, stockCardId, TransactionType.Sell, 3m, 120m, DateTime.UtcNow)
            };
            var transactionGroup = transactions.GroupBy(t => t.StockCardId).First();

            // Act
            var rating = _service.CreateRating(
                transactionGroup,
                period,
                AnalysisContext.Portfolio,
                portfolioId,
                AssetType.Share,
                "SBER",
                "Сбербанк");

            // Assert
            rating.Should().NotBeNull();
            rating.BuyTransactionCount.Should().Be(2);
            rating.SellTransactionCount.Should().Be(1);
            rating.TotalBuyAmount.Should().Be(1550m); // 10*100 + 5*110
            rating.TotalSellAmount.Should().Be(360m); // 3*120
            rating.TotalBuyQuantity.Should().Be(15);
            rating.TotalSellQuantity.Should().Be(3);
        }

        [Fact]
        public void CreateRating_PortfolioContextWithoutPortfolioId_ThrowsArgumentException()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var period = Period.LastDay(DateTime.UtcNow);
            var transactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(Guid.NewGuid(), stockCardId, TransactionType.Buy, 10m, 100m, DateTime.UtcNow)
            };
            var transactionGroup = transactions.GroupBy(t => t.StockCardId).First();

            // Act & Assert
            var act = () => _service.CreateRating(
                transactionGroup,
                period,
                AnalysisContext.Portfolio,
                null,
                AssetType.Share,
                "SBER",
                "Сбербанк");

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PortfolioId*");
        }

        [Fact]
        public void CreateRating_GlobalContextWithPortfolioId_ThrowsArgumentException()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var period = Period.LastDay(DateTime.UtcNow);
            var transactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(Guid.NewGuid(), stockCardId, TransactionType.Buy, 10m, 100m, DateTime.UtcNow)
            };
            var transactionGroup = transactions.GroupBy(t => t.StockCardId).First();

            // Act & Assert
            var act = () => _service.CreateRating(
                transactionGroup,
                period,
                AnalysisContext.Global,
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк");

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Global*");
        }

        [Fact]
        public void AssignRanks_ValidRatings_AssignsRanks()
        {
            // Arrange
            var ratings = new List<AssetRating>
            {
                AssetRating.CreateGlobalRating(Guid.NewGuid(), AssetType.Share, "SBER", "Сбербанк", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
                AssetRating.CreateGlobalRating(Guid.NewGuid(), AssetType.Share, "GAZP", "Газпром", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
                AssetRating.CreateGlobalRating(Guid.NewGuid(), AssetType.Share, "LKOH", "Лукойл", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow)
            };

            // Устанавливаем статистику с минимальными рангами (1, 1), затем AssignRanks перезапишет их
            // Используем UpdateStatistics для установки всех полей, включая количества
            ratings[0].UpdateStatistics(10, 5, 10000m, 5000m, 100, 50, 1, 1);
            ratings[1].UpdateStatistics(8, 4, 8000m, 4000m, 80, 40, 1, 1);
            ratings[2].UpdateStatistics(5, 3, 5000m, 3000m, 50, 30, 1, 1);

            // Act
            _service.AssignRanks(ratings);

            // Assert
            // Проверяем, что ранги перезаписаны правильно
            // SBER имеет наибольшее количество транзакций (15) и наибольшую сумму (15000)
            // AssignRanks сначала устанавливает countRank с amountRank=0, затем обновляет amountRank
            // Но AssignRanks проверяет, что amountRank > 0, поэтому нужно проверить финальные значения
            var sberRating = ratings[0]; // SBER - наибольшее количество (15) и наибольшая сумма (15000)
            var gazpRating = ratings[1]; // GAZP - среднее количество (12) и средняя сумма (12000)
            var lkohRating = ratings[2]; // LKOH - наименьшее количество (8) и наименьшая сумма (8000)

            sberRating.TransactionCountRank.Should().Be(1);
            gazpRating.TransactionCountRank.Should().Be(2);
            lkohRating.TransactionCountRank.Should().Be(3);

            sberRating.TransactionAmountRank.Should().Be(1);
            gazpRating.TransactionAmountRank.Should().Be(2);
            lkohRating.TransactionAmountRank.Should().Be(3);
        }

        [Fact]
        public void CalculateIncrementalUpdate_BuyTransaction_UpdatesBuyCounters()
        {
            // Arrange
            var existingRating = AssetRating.CreateGlobalRating(
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            existingRating.UpdateStatistics(5, 3, 5000m, 3000m, 50, 30, 1, 1);

            var newTransaction = AssetTransaction.Create(
                Guid.NewGuid(),
                existingRating.StockCardId,
                TransactionType.Buy,
                10m,
                100m,
                DateTime.UtcNow);

            // Act
            var updatedRating = _service.CalculateIncrementalUpdate(existingRating, newTransaction);

            // Assert
            updatedRating.BuyTransactionCount.Should().Be(6);
            updatedRating.SellTransactionCount.Should().Be(3);
            updatedRating.TotalBuyAmount.Should().Be(6000m); // 5000 + 1000
            // TotalBuyQuantity и TotalSellQuantity не обновляются через UpdateCounts, только через UpdateStatistics
        }

        [Fact]
        public void CalculateIncrementalUpdate_SellTransaction_UpdatesSellCounters()
        {
            // Arrange
            var existingRating = AssetRating.CreateGlobalRating(
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            existingRating.UpdateStatistics(5, 3, 5000m, 3000m, 50, 30, 1, 1);

            var newTransaction = AssetTransaction.Create(
                Guid.NewGuid(),
                existingRating.StockCardId,
                TransactionType.Sell,
                5m,
                120m,
                DateTime.UtcNow);

            // Act
            var updatedRating = _service.CalculateIncrementalUpdate(existingRating, newTransaction);

            // Assert
            updatedRating.BuyTransactionCount.Should().Be(5);
            updatedRating.SellTransactionCount.Should().Be(4);
            updatedRating.TotalSellAmount.Should().Be(3600m); // 3000 + 600
            // TotalBuyQuantity и TotalSellQuantity не обновляются через UpdateCounts, только через UpdateStatistics
        }
    }
}

