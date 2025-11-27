namespace StockMarketAssistant.SharedLibrary.Options
{
    /// <summary>
    /// Настройки Kafka
    /// </summary>
    public class KafkaOptions
    {
        /// <summary>
        /// Адреса брокеров Kafka
        /// </summary>
        public string BootstrapServers { get; set; } = string.Empty;
    }
}
