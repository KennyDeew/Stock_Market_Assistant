using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockCardService.WebApi.Controllers;
using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Tests.Builders;
using StockMarketAssistant.StockCardService.WebApi.Mappers;
using Xunit;

namespace StockMarketAssistant.StockCardService.Tests.WebHost
{
    public class ShareCardControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IShareCardService> _shareCardServiceMock;
        private readonly ShareCardController _sut;

        public ShareCardControllerTests()
        {
            _fixture = new Fixture();
            _shareCardServiceMock = new Mock<IShareCardService>();
            _sut = new ShareCardController(_shareCardServiceMock.Object);
        }

        [Fact]
        public async Task GetShareCardsAsync_WhenCalled_ReturnsListOfShareCardModels()
        {
            // Arrange
            var shareCards = new List<ShareCardDto>
            {
                new ShareCardBuilder().WithTicker("SBER").ToDto(),
                new ShareCardBuilder().WithTicker("GAZP").ToDto()
            };

            _shareCardServiceMock
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(shareCards);

            // Act
            var result = await _sut.GetShareCardsAsync();

            // Assert
            var models = result.Value;
            models.Should().HaveCount(2);
            models.First().Ticker.Should().Be("SBER");
            _shareCardServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetShareCardAsync_CardExists_ReturnsShareCardModel()
        {
            // Arrange
            var id = Guid.NewGuid();
            var shareCard = new ShareCardBuilder().WithId(id).WithTicker("SBER").ToDto();

            _shareCardServiceMock
                .Setup(s => s.GetByIdWithLinkedItemsAsync(id))
                .ReturnsAsync(shareCard);

            // Act
            var result = await _sut.GetShareCardAsync(id);

            // Assert
            result.Value.Should().BeEquivalentTo(ShareCardMapper.ToModel(shareCard));
        }

        [Fact]
        public async Task GetShareCardAsync_CardNotExists_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _shareCardServiceMock
                .Setup(s => s.GetByIdWithLinkedItemsAsync(id))
                .ReturnsAsync((ShareCardDto?)null);

            // Act
            var result = await _sut.GetShareCardAsync(id);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetShortShareCardModelAsync_CardExists_ReturnsOkWithShortModel()
        {
            // Arrange
            var id = Guid.NewGuid();
            var shareCard = new ShareCardBuilder().WithId(id).WithTicker("SBER").ToShortDto();

            _shareCardServiceMock
                .Setup(s => s.GetShortByIdAsync(id))
                .ReturnsAsync(shareCard);

            // Act
            var result = await _sut.GetShortShareCardModelAsync(id);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(ShareCardMapper.ToModel(shareCard));
        }

        [Fact]
        public async Task GetShortShareCardModelAsync_CardNotExists_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _shareCardServiceMock
                .Setup(s => s.GetShortByIdAsync(id))
                .ReturnsAsync((ShareCardShortDto?)null);

            // Act
            var result = await _sut.GetShortShareCardModelAsync(id);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateShareCard_WhenValidRequest_ReturnsCreatedAtRoute()
        {
            // Arrange
            var newId = Guid.NewGuid();
            var request = new CreatingShareCardModel
            {
                Ticker = "SBER",
                Name = "Сбербанк",
                Description = "Банк",
                Currency = "RUB"
            };

            _shareCardServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<CreatingShareCardDto>()))
                .ReturnsAsync(newId);

            // Act
            var result = await _sut.CreateShareCard(request);

            // Assert
            var createdResult = result as CreatedAtRouteResult;
            createdResult.Should().NotBeNull();
            createdResult!.RouteName.Should().Be("GetShareCardShortModel");
            createdResult.RouteValues!["id"].Should().Be(newId);
        }

        [Fact]
        public async Task EditShareCardAsync_WhenCalled_ReturnsOk()
        {
            // Arrange
            var request = new UpdatingShareCardModel
            {
                Id = Guid.NewGuid(),
                Ticker = "SBER",
                Name = "Сбербанк",
                Description = "Банк",
                CurrentPrice = 100
            };

            _shareCardServiceMock
                .Setup(s => s.UpdateAsync(It.IsAny<UpdatingShareCardDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.EditShareCardAsync(request);

            // Assert
            result.Should().BeOfType<OkResult>();
            _shareCardServiceMock.Verify(s => s.UpdateAsync(It.IsAny<UpdatingShareCardDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAllShareCardPriceAsync_WhenCalled_ReturnsOk()
        {
            // Arrange
            _shareCardServiceMock
                .Setup(s => s.UpdateShareCardPricesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAllShareCardPriceAsync();

            // Assert
            result.Should().BeOfType<OkResult>();
            _shareCardServiceMock.Verify(s => s.UpdateShareCardPricesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteShareCard_WhenCalled_ReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _shareCardServiceMock
                .Setup(s => s.DeleteAsync(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteShareCard(id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _shareCardServiceMock.Verify(s => s.DeleteAsync(id), Times.Once);
        }
    }
}