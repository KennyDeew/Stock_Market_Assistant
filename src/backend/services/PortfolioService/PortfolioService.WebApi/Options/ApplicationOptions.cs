using StockMarketAssistant.SharedLibrary.Options;

namespace StockMarketAssistant.PortfolioService.WebApi.Options
{
    /// <summary>
    /// Настройки Application
    /// </summary>
    public class ApplicationOptions
    {
        /// <summary>
        /// Настройки Kafka
        /// </summary>
        public KafkaOptions? KafkaOptions { get; set; }
    }
}
