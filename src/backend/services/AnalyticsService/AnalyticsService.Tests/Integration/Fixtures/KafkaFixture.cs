using Testcontainers.Kafka;

namespace StockMarketAssistant.AnalyticsService.Tests.Integration.Fixtures
{
    /// <summary>
    /// Фикстура для Kafka контейнера в интеграционных тестах
    /// </summary>
    public class KafkaFixture : IAsyncLifetime
    {
        private readonly KafkaContainer _kafkaContainer;
        private string _bootstrapServers = string.Empty;

        public KafkaFixture()
        {
            _kafkaContainer = new KafkaBuilder()
                .WithImage("confluentinc/cp-kafka:7.5.0")
                .Build();
        }

        public string BootstrapServers => _bootstrapServers;

        public async Task InitializeAsync()
        {
            await _kafkaContainer.StartAsync();
            _bootstrapServers = _kafkaContainer.GetBootstrapAddress();
        }

        public async Task DisposeAsync()
        {
            await _kafkaContainer.DisposeAsync();
        }
    }
}

