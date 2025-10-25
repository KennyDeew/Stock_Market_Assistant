using AutoFixture;
using FluentAssertions;
using Moq;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier;
using StockMarketAssistant.StockCardService.Application.Services;
using StockMarketAssistant.StockCardService.Domain.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Tests.Builders;

namespace StockMarketAssistant.StockCardService.Tests.Application
{
    public class ShareCardServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IRepository<ShareCard, Guid>> _shareCardRepositoryMock;
        private readonly ShareCardService _sut; // System Under Test

        public ShareCardServiceTests()
        {
            _fixture = new Fixture();
            _shareCardRepositoryMock = new Mock<IRepository<ShareCard, Guid>>();
            _sut = new ShareCardService(_shareCardRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsCards_ReturnsMappedDtos()
        {
            // Arrange
            var shareCards = new List<ShareCard>
            {
                new ShareCardBuilder().WithTicker("SBER").Build(),
                new ShareCardBuilder().WithTicker("GAZP").Build()
            };

            _shareCardRepositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>(), false))
                .ReturnsAsync(shareCards);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            result.Should().NotBeNull().And.HaveCount(2);
            result.First().Ticker.Should().Be("SBER");
            _shareCardRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>(), false), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_CardExists_ReturnsDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var shareCard = new ShareCardBuilder().WithId(id).WithTicker("SBER").Build();

            _shareCardRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(shareCard);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Ticker.Should().Be("SBER");
        }

        [Fact]
        public async Task GetByIdAsync_CardDoesNotExist_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _shareCardRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShareCard?)null);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsNewId()
        {
            // Arrange
            var dto = _fixture.Create<CreatingShareCardDto>();
            var created = new ShareCardBuilder().WithId(Guid.NewGuid()).WithTicker(dto.Ticker).Build();

            _shareCardRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ShareCard>()))
                .ReturnsAsync(created);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().Be(created.Id);
            _shareCardRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ShareCard>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_RepositoryCalledOnce()
        {
            // Arrange
            var id = Guid.NewGuid();

            _shareCardRepositoryMock
                .Setup(r => r.DeleteAsync(id))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteAsync(id);

            // Assert
            _shareCardRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CardExists_UpdatesEntity()
        {
            // Arrange
            var cardId = Guid.NewGuid();
            var existing = new ShareCardBuilder().WithId(cardId).WithTicker("SBER").Build();
            var dto = new UpdatingShareCardDto
            {
                Id = cardId,
                Ticker = "GAZP",
                Name = "Газпром",
                Description = "Газовая компания",
                CurrentPrice = 300
            };

            _shareCardRepositoryMock
                .Setup(r => r.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _shareCardRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<ShareCard>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            existing.Ticker.Should().Be("GAZP");
            existing.CurrentPrice.Should().Be(300);
            _shareCardRepositoryMock.Verify(r => r.UpdateAsync(It.Is<ShareCard>(s => s.Ticker == "GAZP")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CardNotFound_DoesNothing()
        {
            // Arrange
            var dto = _fixture.Create<UpdatingShareCardDto>();

            _shareCardRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShareCard?)null);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            _shareCardRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ShareCard>()), Times.Never);
        }

        [Fact]
        public async Task GetShortByIdAsync_CardExists_ReturnsShortDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var card = new ShareCardBuilder().WithId(id).WithTicker("TCSG").Build();

            _shareCardRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(card);

            // Act
            var result = await _sut.GetShortByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Ticker.Should().Be("TCSG");
        }

        [Fact]
        public async Task GetShortByIdAsync_CardNotFound_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _shareCardRepositoryMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShareCard?)null);

            // Act
            var result = await _sut.GetShortByIdAsync(id);

            // Assert
            result.Should().BeNull();
        }
    }
}