using Moq;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Domain.Enums;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;
using Xunit;

namespace StockMarketAssistant.PortfolioService.UnitTests
{
    public class PortfolioAssetAppServiceTests
    {
        private readonly Mock<IPortfolioAssetRepository> _assetRepoMock = new();
        private readonly Mock<IPortfolioRepository> _portfolioRepoMock = new();
        private readonly Mock<IUserContext> _userContextMock = new();
        private readonly Mock<IStockCardServiceGateway> _gatewayMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly PortfolioAssetAppService _service;

        public PortfolioAssetAppServiceTests()
        {
            _service = new PortfolioAssetAppService(
                _assetRepoMock.Object,
                _userContextMock.Object,
                _portfolioRepoMock.Object,
                _gatewayMock.Object,
                _cacheMock.Object,
                null!
            );
        }

        [Fact]
        public async Task CreateAsync_UserCreatesAssetInOtherUserPortfolio_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new CreatingPortfolioAssetDto(Guid.NewGuid(), Guid.NewGuid(), PortfolioAssetType.Share, 100, 10);
            var portfolio = new Portfolio(Guid.NewGuid(), Guid.NewGuid(), "Test portfolio");

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(dto.PortfolioId))
                .ReturnsAsync(portfolio);
            _userContextMock.Setup(u => u.IsAdmin).Returns(false);
            _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task DeleteAsync_UserDeletesOtherUserAsset_ThrowsSecurityException()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var asset = new PortfolioAsset(assetId, Guid.NewGuid(), Guid.NewGuid(), PortfolioAssetType.Share);
            var portfolio = new Portfolio(asset.PortfolioId, Guid.NewGuid(), "Test portfolio");

            _assetRepoMock.Setup(r => r.GetByIdAsync(assetId)).ReturnsAsync(asset);
            _portfolioRepoMock.Setup(r => r.GetByIdAsync(asset.PortfolioId)).ReturnsAsync(portfolio);
            _userContextMock.Setup(u => u.IsAdmin).Returns(false);
            _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => _service.DeleteAsync(assetId));
        }
    }
}
