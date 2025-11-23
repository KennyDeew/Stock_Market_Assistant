using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Tests.Domain
{
    public class AssetTransactionTests
    {
        [Fact]
        public void Create_ValidParameters_ReturnsTransaction()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = 10m;
            var price = 100m;
            var transactionDate = DateTime.UtcNow;

            // Act
            var transaction = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            // Assert
            transaction.Should().NotBeNull();
            transaction.PortfolioId.Should().Be(portfolioId);
            transaction.StockCardId.Should().Be(stockCardId);
            transaction.TransactionType.Should().Be(transactionType);
            transaction.Quantity.Should().Be((int)quantity);
            transaction.PricePerUnit.Should().Be(price);
            transaction.TotalAmount.Should().Be(quantity * price);
            transaction.TransactionTime.Should().BeCloseTo(transactionDate, TimeSpan.FromSeconds(1));
            transaction.Currency.Should().Be("RUB");
        }

        [Fact]
        public void Create_EmptyPortfolioId_ThrowsArgumentException()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = 10m;
            var price = 100m;
            var transactionDate = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.Create(
                Guid.Empty,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PortfolioId*");
        }

        [Fact]
        public void Create_EmptyStockCardId_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = 10m;
            var price = 100m;
            var transactionDate = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.Create(
                portfolioId,
                Guid.Empty,
                transactionType,
                quantity,
                price,
                transactionDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*StockCardId*");
        }

        [Fact]
        public void Create_ZeroQuantity_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = 0m;
            var price = 100m;
            var transactionDate = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.Create(
                portfolioId,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Quantity*");
        }

        [Fact]
        public void Create_NegativeQuantity_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = -10m;
            var price = 100m;
            var transactionDate = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.Create(
                portfolioId,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Quantity*");
        }

        [Fact]
        public void Create_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = 10m;
            var price = -100m;
            var transactionDate = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.Create(
                portfolioId,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Price*");
        }

        [Fact]
        public void Create_FutureTransactionDate_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Buy;
            var quantity = 10m;
            var price = 100m;
            var transactionDate = DateTime.UtcNow.AddDays(1);

            // Act & Assert
            var act = () => AssetTransaction.Create(
                portfolioId,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*транзакции*");
        }

        [Fact]
        public void Create_SellTransaction_CalculatesTotalAmountCorrectly()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionType = TransactionType.Sell;
            var quantity = 5m;
            var price = 150m;
            var transactionDate = DateTime.UtcNow;

            // Act
            var transaction = AssetTransaction.Create(
                portfolioId,
                stockCardId,
                transactionType,
                quantity,
                price,
                transactionDate);

            // Assert
            transaction.TransactionType.Should().Be(TransactionType.Sell);
            transaction.TotalAmount.Should().Be(quantity * price);
        }

        [Fact]
        public void CreateBuyTransaction_ValidParameters_ReturnsBuyTransaction()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow;
            var currency = "RUB";
            var metadata = "Test metadata";

            // Act
            var transaction = AssetTransaction.CreateBuyTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime,
                currency,
                metadata);

            // Assert
            transaction.Should().NotBeNull();
            transaction.PortfolioId.Should().Be(portfolioId);
            transaction.StockCardId.Should().Be(stockCardId);
            transaction.AssetType.Should().Be(assetType);
            transaction.TransactionType.Should().Be(TransactionType.Buy);
            transaction.Quantity.Should().Be(quantity);
            transaction.PricePerUnit.Should().Be(pricePerUnit);
            transaction.TotalAmount.Should().Be(quantity * pricePerUnit);
            transaction.TransactionTime.Should().BeCloseTo(transactionTime, TimeSpan.FromSeconds(1));
            transaction.Currency.Should().Be(currency);
            transaction.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void CreateBuyTransaction_EmptyPortfolioId_ThrowsArgumentException()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                Guid.Empty,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PortfolioId*");
        }

        [Fact]
        public void CreateBuyTransaction_EmptyStockCardId_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                portfolioId,
                Guid.Empty,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*StockCardId*");
        }

        [Fact]
        public void CreateBuyTransaction_ZeroQuantity_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 0;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Quantity*");
        }

        [Fact]
        public void CreateBuyTransaction_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = -100m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Price*");
        }

        [Fact]
        public void CreateBuyTransaction_EmptyCurrency_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow;
            var currency = string.Empty;

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime,
                currency);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Currency*");
        }

        [Fact]
        public void CreateBuyTransaction_CurrencyTooLong_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow;
            var currency = new string('A', 11); // 11 символов

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime,
                currency);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Currency*");
        }

        [Fact]
        public void CreateBuyTransaction_FutureTransactionTime_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 10;
            var pricePerUnit = 100m;
            var transactionTime = DateTime.UtcNow.AddDays(1);

            // Act & Assert
            var act = () => AssetTransaction.CreateBuyTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*транзакции*");
        }

        [Fact]
        public void CreateSellTransaction_ValidParameters_ReturnsSellTransaction()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Bond;
            var quantity = 5;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow;
            var currency = "USD";
            var metadata = "Sell metadata";

            // Act
            var transaction = AssetTransaction.CreateSellTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime,
                currency,
                metadata);

            // Assert
            transaction.Should().NotBeNull();
            transaction.PortfolioId.Should().Be(portfolioId);
            transaction.StockCardId.Should().Be(stockCardId);
            transaction.AssetType.Should().Be(assetType);
            transaction.TransactionType.Should().Be(TransactionType.Sell);
            transaction.Quantity.Should().Be(quantity);
            transaction.PricePerUnit.Should().Be(pricePerUnit);
            transaction.TotalAmount.Should().Be(quantity * pricePerUnit);
            transaction.TransactionTime.Should().BeCloseTo(transactionTime, TimeSpan.FromSeconds(1));
            transaction.Currency.Should().Be(currency);
            transaction.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void CreateSellTransaction_EmptyPortfolioId_ThrowsArgumentException()
        {
            // Arrange
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 5;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                Guid.Empty,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*PortfolioId*");
        }

        [Fact]
        public void CreateSellTransaction_EmptyStockCardId_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 5;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                portfolioId,
                Guid.Empty,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*StockCardId*");
        }

        [Fact]
        public void CreateSellTransaction_ZeroQuantity_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 0;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Quantity*");
        }

        [Fact]
        public void CreateSellTransaction_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 5;
            var pricePerUnit = -150m;
            var transactionTime = DateTime.UtcNow;

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Price*");
        }

        [Fact]
        public void CreateSellTransaction_EmptyCurrency_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 5;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow;
            var currency = string.Empty;

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime,
                currency);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Currency*");
        }

        [Fact]
        public void CreateSellTransaction_CurrencyTooLong_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 5;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow;
            var currency = new string('B', 11); // 11 символов

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime,
                currency);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Currency*");
        }

        [Fact]
        public void CreateSellTransaction_FutureTransactionTime_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var assetType = AssetType.Share;
            var quantity = 5;
            var pricePerUnit = 150m;
            var transactionTime = DateTime.UtcNow.AddDays(1);

            // Act & Assert
            var act = () => AssetTransaction.CreateSellTransaction(
                portfolioId,
                stockCardId,
                assetType,
                quantity,
                pricePerUnit,
                transactionTime);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*транзакции*");
        }
    }
}

