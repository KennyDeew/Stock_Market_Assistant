using Microsoft.Extensions.Logging;
using Moq;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Application.UseCases;
using StockMarketAssistant.AnalyticsService.Domain.Constants;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Tests.Application
{
    public class GetTopSoldAssetsUseCaseTests
    {
        private readonly Mock<IAssetRatingRepository> _repositoryMock;
        private readonly Mock<ILogger<GetTopSoldAssetsUseCase>> _loggerMock;
        private readonly GetTopSoldAssetsUseCase _useCase;

        public GetTopSoldAssetsUseCaseTests()
        {
            _repositoryMock = new Mock<IAssetRatingRepository>();
            _loggerMock = new Mock<ILogger<GetTopSoldAssetsUseCase>>();
            _useCase = new GetTopSoldAssetsUseCase(_repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ValidParameters_ReturnsTopAssets()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var top = 5;
            var context = AnalysisContext.Global;
            var period = Period.Custom(startDate, endDate);

            var expectedRatings = new List<AssetRating>
            {
                AssetRating.CreateGlobalRating(Guid.NewGuid(), AssetType.Share, "SBER", "Сбербанк", startDate, endDate),
                AssetRating.CreateGlobalRating(Guid.NewGuid(), AssetType.Share, "GAZP", "Газпром", startDate, endDate)
            };

            _repositoryMock
                .Setup(r => r.GetTopSoldAsync(top, It.IsAny<Period>(), context, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRatings);

            // Act
            var result = await _useCase.ExecuteAsync(startDate, endDate, top, context);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            _repositoryMock.Verify(r => r.GetTopSoldAsync(top, It.IsAny<Period>(), context, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_StartDateAfterEndDate_ThrowsArgumentException()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(-7);

            // Act & Assert
            var act = async () => await _useCase.ExecuteAsync(startDate, endDate);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*начальная дата*");
        }

        [Fact]
        public async Task ExecuteAsync_TopLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var top = 0;

            // Act & Assert
            var act = async () => await _useCase.ExecuteAsync(startDate, endDate, top);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
                .WithParameterName("top");
        }
    }
}

