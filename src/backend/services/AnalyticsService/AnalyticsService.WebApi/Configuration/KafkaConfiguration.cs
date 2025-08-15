using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.AnalyticsService.WebApi.Configuration
{
    /// <summary>
    /// Конфигурация Kafka
    /// </summary>
    public class KafkaConfiguration
    {
        /// <summary>
        /// Серверы Kafka
        /// </summary>
        [Required]
        public string BootstrapServers { get; set; } = string.Empty;

        /// <summary>
        /// ID группы потребителей
        /// </summary>
        [Required]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// Топик для транзакций портфеля
        /// </summary>
        [Required]
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Автоматический сброс смещения
        /// </summary>
        public string AutoOffsetReset { get; set; } = "Earliest";

        /// <summary>
        /// Автоматический коммит
        /// </summary>
        public bool EnableAutoCommit { get; set; } = false;

        /// <summary>
        /// Имя пользователя SASL (получается из секретов)
        /// </summary>
        public string SaslUsername { get; set; } = string.Empty;

        /// <summary>
        /// Пароль SASL (получается из секретов)
        /// </summary>
        public string SaslPassword { get; set; } = string.Empty;

        /// <summary>
        /// Механизм SASL
        /// </summary>
        public string SaslMechanism { get; set; } = "PLAIN";

        /// <summary>
        /// Протокол безопасности
        /// </summary>
        public string SecurityProtocol { get; set; } = "PLAINTEXT";

        /// <summary>
        /// Проверить, включена ли аутентификация SASL
        /// </summary>
        /// <returns>True, если аутентификация SASL включена</returns>
        public bool IsSaslEnabled()
        {
            return !string.IsNullOrEmpty(SaslUsername) && !string.IsNullOrEmpty(SaslPassword);
        }
    }
}
