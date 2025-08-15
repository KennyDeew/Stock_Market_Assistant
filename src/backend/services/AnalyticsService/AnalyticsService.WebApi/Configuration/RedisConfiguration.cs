using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.AnalyticsService.WebApi.Configuration
{
    /// <summary>
    /// Конфигурация Redis
    /// </summary>
    public class RedisConfiguration
    {
        /// <summary>
        /// Строка подключения к Redis
        /// </summary>
        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Номер базы данных Redis
        /// </summary>
        [Range(0, 15)]
        public int Database { get; set; } = 0;

        /// <summary>
        /// Префикс ключей
        /// </summary>
        public string KeyPrefix { get; set; } = "asset-prices:";

        /// <summary>
        /// Пароль Redis (получается из секретов)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Хост Redis
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Порт Redis
        /// </summary>
        [Range(1, 65535)]
        public int Port { get; set; } = 6379;

        /// <summary>
        /// Получить строку подключения с паролем
        /// </summary>
        /// <returns>Строка подключения к Redis</returns>
        public string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                return $"{Host}:{Port},password={Password},defaultDatabase={Database}";
            }
            
            return $"{Host}:{Port},defaultDatabase={Database}";
        }

        /// <summary>
        /// Получить строку подключения без пароля (для логирования)
        /// </summary>
        /// <returns>Строка подключения без пароля</returns>
        public string GetConnectionStringForLogging()
        {
            if (!string.IsNullOrEmpty(Password))
            {
                return $"{Host}:{Port},password=***,defaultDatabase={Database}";
            }
            
            return $"{Host}:{Port},defaultDatabase={Database}";
        }
    }
}
