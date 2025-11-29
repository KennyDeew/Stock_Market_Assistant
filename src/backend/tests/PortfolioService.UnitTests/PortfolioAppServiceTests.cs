using Moq;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Domain.Exceptions;
using Xunit;

namespace StockMarketAssistant.PortfolioService.UnitTests
{
    public class PortfolioAppServiceTests
    {
        private readonly Mock<IPortfolioRepository> _portfolioRepoMock = new();
        private readonly Mock<IUserContext> _userContextMock = new();
        private readonly Mock<IPortfolioAssetAppService> _assetAppServiceMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly PortfolioAppService _service;

        public PortfolioAppServiceTests()
        {
            _service = new PortfolioAppService(
                _portfolioRepoMock.Object,
                _userContextMock.Object,
                _assetAppServiceMock.Object,
                _cacheMock.Object,
                null! // ILogger можно игнорировать в юнит-тестах
            );
        }

        [Fact]
        public async Task CreateAsync_UserCreatesForOtherUser_ThrowsSecurityException()
        {
            // Arrange
            var dto = new CreatingPortfolioDto(Guid.NewGuid(), "Test portfolio");
            _userContextMock.Setup(u => u.IsAdmin).Returns(false);
            _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task DeleteAsync_UserDeletesOtherUserPortfolio_ThrowsSecurityException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var portfolio = new Portfolio(portfolioId, Guid.NewGuid(), "Test portfolio");

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolioId))
                .ReturnsAsync(portfolio);
            _userContextMock.Setup(u => u.IsAdmin).Returns(false);
            _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<SecurityException>(() => _service.DeleteAsync(portfolioId));
        }

        [Fact]
        public async Task GetByIdAsync_UserAccessesOtherUserPortfolio_ReturnsNull()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var portfolio = new Portfolio(portfolioId, Guid.NewGuid(), "Test portfolio");

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolioId))
                .ReturnsAsync(portfolio);
            _userContextMock.Setup(u => u.IsAdmin).Returns(false);
            _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            // Act
            var result = await _service.GetByIdAsync(portfolioId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_UserUpdatesOtherUserPortfolio_ThrowsKeyNotFoundException()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var portfolio = new Portfolio(portfolioId, Guid.NewGuid(), "Test portfolio");
            var updateDto = new UpdatingPortfolioDto("New", "USD");

            _portfolioRepoMock.Setup(r => r.GetByIdAsync(portfolioId))
                .ReturnsAsync(portfolio);
            _userContextMock.Setup(u => u.IsAdmin).Returns(false);
            _userContextMock.Setup(u => u.UserId).Returns(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(portfolioId, updateDto));
        }
    }
}
