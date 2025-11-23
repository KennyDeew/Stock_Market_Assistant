using System.Text.Json;
using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Kafka;
using StockMarketAssistant.AnalyticsService.Tests.Integration.Fixtures;
using Xunit;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Kafka
{
    /// <summary>
    /// Интеграционные тесты для TransactionConsumer с реальным Kafka
    /// </summary>
    [Collection("Kafka")]
    public class TransactionConsumerTests : IClassFixture<KafkaFixture>, IDisposable
    {
        private readonly KafkaFixture _kafkaFixture;
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IAssetTransactionRepository> _repositoryMock;
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<ILogger<TransactionConsumer>> _loggerMock;
        private readonly KafkaConfiguration _config;
        private readonly TransactionConsumer _consumerService;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TransactionConsumerTests(KafkaFixture kafkaFixture)
        {
            _kafkaFixture = kafkaFixture;
            _cancellationTokenSource = new CancellationTokenSource();

            // Настройка Kafka Consumer
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaFixture.BootstrapServers,
                GroupId = "test-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            // Настройка Kafka Producer
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _kafkaFixture.BootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(producerConfig).Build();

            // Настройка моков
            _repositoryMock = new Mock<IAssetTransactionRepository>();
            _eventBusMock = new Mock<IEventBus>();
            _loggerMock = new Mock<ILogger<TransactionConsumer>>();

            _serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactoryMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IAssetTransactionRepository)))
                .Returns(_repositoryMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventBus)))
                .Returns(_eventBusMock.Object);

            // Настройка конфигурации
            _config = new KafkaConfiguration
            {
                BootstrapServers = _kafkaFixture.BootstrapServers,
                ConsumerGroup = "test-consumer-group",
                Topic = "test-portfolio.transactions",
                BatchSize = 10
            };

            _consumerService = new TransactionConsumer(
                _consumer,
                Options.Create(_config),
                _loggerMock.Object,
                _serviceProviderMock.Object,
                null, // DLQ Producer не используется в тестах
                _eventBusMock.Object);
        }

        [Fact]
        public async Task ProcessBatchAsync_ValidMessage_SavesTransaction()
        {
            // Arrange
            var portfolioId = Guid.NewGuid();
            var stockCardId = Guid.NewGuid();
            var transactionDate = DateTime.UtcNow;

            var message = new TransactionMessage
            {
                Id = Guid.NewGuid(),
                PortfolioId = portfolioId,
                StockCardId = stockCardId,
                AssetType = 1, // Share
                TransactionType = 1, // Buy
                Quantity = 10,
                PricePerUnit = 100m,
                Currency = "RUB",
                TotalAmount = 1000m,
                TransactionTime = transactionDate
            };

            var messageJson = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = portfolioId.ToString(),
                Value = messageJson
            };

            // Отправляем сообщение в Kafka
            await _producer.ProduceAsync(_config.Topic, kafkaMessage);

            // Ждем немного, чтобы сообщение было доставлено
            await Task.Delay(2000);

            // Act
            // Запускаем consumer в фоне
            var consumerTask = _consumerService.StartAsync(_cancellationTokenSource.Token);

            // Ждем обработки
            await Task.Delay(5000);

            // Останавливаем consumer
            _cancellationTokenSource.Cancel();

            try
            {
                await consumerTask;
            }
            catch (OperationCanceledException)
            {
                // Ожидаемое исключение при остановке
            }

            // Assert
            _repositoryMock.Verify(
                r => r.AddAsync(It.Is<AssetTransaction>(t =>
                    t.PortfolioId == portfolioId &&
                    t.StockCardId == stockCardId &&
                    t.TransactionType == TransactionType.Buy &&
                    t.Quantity == 10),
                It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            _repositoryMock.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            _eventBusMock.Verify(
                e => e.PublishAsync(It.IsAny<StockMarketAssistant.AnalyticsService.Domain.Events.TransactionReceivedEvent>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessBatchAsync_InvalidMessage_HandlesGracefully()
        {
            // Arrange
            var invalidMessage = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = "invalid json"
            };

            // Отправляем невалидное сообщение
            await _producer.ProduceAsync(_config.Topic, invalidMessage);

            // Ждем немного
            await Task.Delay(2000);

            // Act
            var consumerTask = _consumerService.StartAsync(_cancellationTokenSource.Token);

            await Task.Delay(5000);
            _cancellationTokenSource.Cancel();

            try
            {
                await consumerTask;
            }
            catch (OperationCanceledException)
            {
                // Ожидаемое исключение при остановке
            }

            // Assert
            // Consumer должен обработать ошибку без падения
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _consumer?.Dispose();
            _producer?.Dispose();
        }
    }

    /// <summary>
    /// Коллекция для Kafka фикстуры
    /// </summary>
    [CollectionDefinition("Kafka")]
    public class KafkaCollection : ICollectionFixture<KafkaFixture>
    {
    }
}

