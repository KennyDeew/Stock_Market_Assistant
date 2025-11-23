using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Tests.Domain
{
    public class AssetRatingTests
    {
        [Fact]
        public void CreatePortfolioRating_ValidParameters_ReturnsRating()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act
            var rating = AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            // Assert
            rating.Should().NotBeNull();
            rating.PortfolioId.Should().Be(portfolioId);
            rating.StockCardId.Should().Be(stockCardId);
            rating.AssetType.Should().Be(assetType);
            rating.Ticker.Should().Be(ticker);
            rating.Name.Should().Be(name);
            rating.PeriodStart.Should().BeCloseTo(periodStart, TimeSpan.FromSeconds(1));
            rating.PeriodEnd.Should().BeCloseTo(periodEnd, TimeSpan.FromSeconds(1));
            rating.Context.Should().Be(AnalysisContext.Portfolio);
        }

        [Fact]
        public void CreateGlobalRating_ValidParameters_ReturnsRating()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "GAZP";
            var name = "Газпром";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act
            var rating = AssetRating.CreateGlobalRating(
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            // Assert
            rating.Should().NotBeNull();
            rating.PortfolioId.Should().BeNull();
            rating.StockCardId.Should().Be(stockCardId);
            rating.AssetType.Should().Be(assetType);
            rating.Ticker.Should().Be(ticker);
            rating.Name.Should().Be(name);
            rating.Context.Should().Be(AnalysisContext.Global);
        }

        [Fact]
        public void CreatePortfolioRating_EmptyPortfolioId_ThrowsArgumentException()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                Guid.Empty,
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PortfolioId*");
        }

        [Fact]
        public void CreatePortfolioRating_EmptyTicker_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                string.Empty,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Ticker*");
        }

        [Fact]
        public void CreatePortfolioRating_InvalidPeriod_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow;
            var periodEnd = DateTime.UtcNow.AddDays(-7); // Конец раньше начала

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PeriodStart*");
        }

        [Fact]
        public void UpdateStatistics_ValidParameters_UpdatesProperties()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            var buyCount = 5;
            var sellCount = 3;
            var buyAmount = 1000m;
            var sellAmount = 600m;
            var buyQuantity = 10;
            var sellQuantity = 6;
            var countRank = 1;
            var amountRank = 2;

            // Act
            rating.UpdateStatistics(
                buyCount,
                sellCount,
                buyAmount,
                sellAmount,
                buyQuantity,
                sellQuantity,
                countRank,
                amountRank);

            // Assert
            rating.BuyTransactionCount.Should().Be(buyCount);
            rating.SellTransactionCount.Should().Be(sellCount);
            rating.TotalBuyAmount.Should().Be(buyAmount);
            rating.TotalSellAmount.Should().Be(sellAmount);
            rating.TotalBuyQuantity.Should().Be(buyQuantity);
            rating.TotalSellQuantity.Should().Be(sellQuantity);
            rating.TransactionCountRank.Should().Be(countRank);
            rating.TransactionAmountRank.Should().Be(amountRank);
        }

        [Fact]
        public void AssignRanks_ValidRanks_UpdatesRanks()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            var countRank = 5;
            var amountRank = 3;

            // Act
            rating.AssignRanks(countRank, amountRank);

            // Assert
            rating.TransactionCountRank.Should().Be(countRank);
            rating.TransactionAmountRank.Should().Be(amountRank);
        }

        [Fact]
        public void MarkAsUpdated_UpdatesLastUpdated()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            var originalLastUpdated = rating.LastUpdated;
            Thread.Sleep(100); // Небольшая задержка для проверки обновления времени

            // Act
            rating.MarkAsUpdated();

            // Assert
            rating.LastUpdated.Should().BeAfter(originalLastUpdated);
        }

        [Fact]
        public void CreatePortfolioRating_EmptyStockCardId_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                Guid.Empty,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*StockCardId*");
        }

        [Fact]
        public void CreatePortfolioRating_TickerTooLong_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = new string('A', 21); // 21 символ
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Ticker*");
        }

        [Fact]
        public void CreatePortfolioRating_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                ticker,
                string.Empty,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Name*");
        }

        [Fact]
        public void CreatePortfolioRating_NameTooLong_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var name = new string('A', 256); // 256 символов
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Name*");
        }

        [Fact]
        public void CreatePortfolioRating_PeriodEndInFuture_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var ticker = "SBER";
            var name = "Сбербанк";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow.AddDays(2); // В будущем

            // Act & Assert
            var act = () => AssetRating.CreatePortfolioRating(
                portfolioId,
                stockCardId,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PeriodEnd*");
        }

        [Fact]
        public void CreateGlobalRating_EmptyStockCardId_ThrowsArgumentException()
        {
            // Arrange
            var assetType = AssetType.Share;
            var ticker = "GAZP";
            var name = "Газпром";
            var periodStart = DateTime.UtcNow.AddDays(-7);
            var periodEnd = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetRating.CreateGlobalRating(
                Guid.Empty,
                assetType,
                ticker,
                name,
                periodStart,
                periodEnd);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*StockCardId*");
        }

        [Fact]
        public void UpdateCounts_NegativeBuyCount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateCounts(-1, 0, 0m, 0m);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*buyCount*");
        }

        [Fact]
        public void UpdateCounts_NegativeSellCount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateCounts(0, -1, 0m, 0m);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*sellCount*");
        }

        [Fact]
        public void UpdateCounts_NegativeBuyAmount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateCounts(0, 0, -100m, 0m);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*buyAmount*");
        }

        [Fact]
        public void UpdateCounts_NegativeSellAmount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateCounts(0, 0, 0m, -100m);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*sellAmount*");
        }

        [Fact]
        public void AssignRanks_ZeroCountRank_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.AssignRanks(0, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*countRank*");
        }

        [Fact]
        public void AssignRanks_ZeroAmountRank_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.AssignRanks(1, 0);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*amountRank*");
        }

        [Fact]
        public void UpdateStatistics_NegativeBuyTransactionCount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(-1, 0, 0m, 0m, 0, 0, 1, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*buyTransactionCount*");
        }

        [Fact]
        public void UpdateStatistics_NegativeSellTransactionCount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, -1, 0m, 0m, 0, 0, 1, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*sellTransactionCount*");
        }

        [Fact]
        public void UpdateStatistics_NegativeTotalBuyAmount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, 0, -100m, 0m, 0, 0, 1, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*totalBuyAmount*");
        }

        [Fact]
        public void UpdateStatistics_NegativeTotalSellAmount_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, 0, 0m, -100m, 0, 0, 1, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*totalSellAmount*");
        }

        [Fact]
        public void UpdateStatistics_NegativeTotalBuyQuantity_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, 0, 0m, 0m, -10, 0, 1, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*totalBuyQuantity*");
        }

        [Fact]
        public void UpdateStatistics_NegativeTotalSellQuantity_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, 0, 0m, 0m, 0, -10, 1, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*totalSellQuantity*");
        }

        [Fact]
        public void UpdateStatistics_ZeroTransactionCountRank_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, 0, 0m, 0m, 0, 0, 0, 1);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*transactionCountRank*");
        }

        [Fact]
        public void UpdateStatistics_ZeroTransactionAmountRank_ThrowsArgumentException()
        {
            // Arrange
            var rating = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            // Act & Assert
            var act = () => rating.UpdateStatistics(0, 0, 0m, 0m, 0, 0, 1, 0);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*transactionAmountRank*");
        }

        [Fact]
        public void Clone_CreatesExactCopy()
        {
            // Arrange
            var original = AssetRating.CreatePortfolioRating(
                Guid.NewGuid(),
                Guid.NewGuid(),
                AssetType.Share,
                "SBER",
                "Сбербанк",
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow);

            original.UpdateStatistics(10, 5, 10000m, 5000m, 100, 50, 1, 2);

            // Act
            var clone = original.Clone();

            // Assert
            clone.Should().NotBeNull();
            clone.Id.Should().NotBe(original.Id); // Новый ID
            clone.StockCardId.Should().Be(original.StockCardId);
            clone.AssetType.Should().Be(original.AssetType);
            clone.Ticker.Should().Be(original.Ticker);
            clone.Name.Should().Be(original.Name);
            clone.PeriodStart.Should().Be(original.PeriodStart);
            clone.PeriodEnd.Should().Be(original.PeriodEnd);
            clone.BuyTransactionCount.Should().Be(original.BuyTransactionCount);
            clone.SellTransactionCount.Should().Be(original.SellTransactionCount);
            clone.TotalBuyAmount.Should().Be(original.TotalBuyAmount);
            clone.TotalSellAmount.Should().Be(original.TotalSellAmount);
            clone.TotalBuyQuantity.Should().Be(original.TotalBuyQuantity);
            clone.TotalSellQuantity.Should().Be(original.TotalSellQuantity);
            clone.TransactionCountRank.Should().Be(original.TransactionCountRank);
            clone.TransactionAmountRank.Should().Be(original.TransactionAmountRank);
            clone.Context.Should().Be(original.Context);
            clone.PortfolioId.Should().Be(original.PortfolioId);
        }
    }
}

