using Microsoft.Extensions.Logging;
using Moq;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Application.UseCases;
using StockMarketAssistant.AnalyticsService.Domain.Constants;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Tests.Application
{
    public class ComparePortfoliosUseCaseTests
    {
        private readonly Mock<IPortfolioServiceClient> _portfolioServiceClientMock;
        private readonly Mock<IAssetTransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ILogger<ComparePortfoliosUseCase>> _loggerMock;
        private readonly ComparePortfoliosUseCase _useCase;

        public ComparePortfoliosUseCaseTests()
        {
            _portfolioServiceClientMock = new Mock<IPortfolioServiceClient>();
            _transactionRepositoryMock = new Mock<IAssetTransactionRepository>();
            _loggerMock = new Mock<ILogger<ComparePortfoliosUseCase>>();
            _useCase = new ComparePortfoliosUseCase(
                _portfolioServiceClientMock.Object,
                _transactionRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ValidPortfolios_ReturnsComparisonResult()
        {
            // Arrange
            var portfolioId1 = Guid.NewGuid();
            var portfolioId2 = Guid.NewGuid();
            var portfolioIds = new[] { portfolioId1, portfolioId2 };

            var portfolioStates = new Dictionary<Guid, PortfolioStateDto>
            {
                { portfolioId1, new PortfolioStateDto { Id = portfolioId1, Name = "Portfolio 1" } },
                { portfolioId2, new PortfolioStateDto { Id = portfolioId2, Name = "Portfolio 2" } }
            };

            var transactions1 = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId1, Guid.NewGuid(), TransactionType.Buy, 10m, 100m, DateTime.UtcNow)
            };

            var transactions2 = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId2, Guid.NewGuid(), TransactionType.Buy, 5m, 150m, DateTime.UtcNow)
            };

            _portfolioServiceClientMock
                .Setup(c => c.GetMultipleStatesAsync(portfolioIds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolioStates);

            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioIdAsync(portfolioId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions1);

            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioIdAsync(portfolioId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions2);

            // Act
            var result = await _useCase.ExecuteAsync(portfolioIds);

            // Assert
            result.Should().NotBeNull();
            result.PortfolioStates.Should().HaveCount(2);
            result.PortfolioTransactions.Should().HaveCount(2);
            result.PortfolioTransactions[portfolioId1].Should().HaveCount(1);
            result.PortfolioTransactions[portfolioId2].Should().HaveCount(1);
        }

        [Fact]
        public async Task ExecuteAsync_EmptyPortfolioIds_ThrowsArgumentException()
        {
            // Arrange
            var portfolioIds = Array.Empty<Guid>();

            // Act & Assert
            var act = async () => await _useCase.ExecuteAsync(portfolioIds);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*хотя бы один портфель*");
        }

        [Fact]
        public async Task ExecuteAsync_TooManyPortfolios_ThrowsArgumentException()
        {
            // Arrange
            var portfolioIds = Enumerable.Range(0, DomainConstants.Validation.MaxPortfoliosInComparison + 1)
                .Select(_ => Guid.NewGuid())
                .ToArray();

            // Act & Assert
            var act = async () => await _useCase.ExecuteAsync(portfolioIds);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"*{DomainConstants.Validation.MaxPortfoliosInComparison}*");
        }

        [Fact]
        public async Task ExecuteAsync_WithPeriod_FiltersTransactionsByPeriod()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var period = Period.Custom(startDate, endDate);

            var portfolioStates = new Dictionary<Guid, PortfolioStateDto>
            {
                { portfolioId, new PortfolioStateDto { Id = portfolioId, Name = "Portfolio 1" } }
            };

            var transactions = new List<AssetTransaction>
            {
                AssetTransaction.Create(portfolioId, Guid.NewGuid(), TransactionType.Buy, 10m, 100m, DateTime.UtcNow.AddDays(-5))
            };

            _portfolioServiceClientMock
                .Setup(c => c.GetMultipleStatesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolioStates);

            _transactionRepositoryMock
                .Setup(r => r.GetByPortfolioAndPeriodAsync(portfolioId, It.IsAny<Period>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _useCase.ExecuteAsync(new[] { portfolioId }, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            _transactionRepositoryMock.Verify(
                r => r.GetByPortfolioAndPeriodAsync(portfolioId, It.IsAny<Period>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_InvalidPeriod_ThrowsArgumentException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(-7); // Конец раньше начала

            var portfolioStates = new Dictionary<Guid, PortfolioStateDto>
            {
                { portfolioId, new PortfolioStateDto { Id = portfolioId, Name = "Portfolio 1" } }
            };

            _portfolioServiceClientMock
                .Setup(c => c.GetMultipleStatesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(portfolioStates);

            // Act & Assert
            var act = async () => await _useCase.ExecuteAsync(new[] { portfolioId }, startDate, endDate);

            // Ожидаем ArgumentException при валидации периода
            // Но сначала код получает portfolioStates, поэтому нужно настроить мок правильно
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*начальная дата*");
        }
    }
}

