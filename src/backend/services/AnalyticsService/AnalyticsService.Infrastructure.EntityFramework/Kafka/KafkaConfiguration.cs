namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Kafka
{
    /// <summary>
    /// Конфигурация для Kafka Consumer
    /// </summary>
    public class KafkaConfiguration
    {
        /// <summary>
        /// Адреса брокеров Kafka (например, "localhost:9092")
        /// </summary>
        public string BootstrapServers { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор группы потребителей
        /// </summary>
        public string ConsumerGroup { get; set; } = string.Empty;

        /// <summary>
        /// Название топика для подписки
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Размер батча для обработки сообщений
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
}

