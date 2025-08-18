using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.AnalyticsService.WebApi.Configuration
{
    /// <summary>
    /// Конфигурация базы данных
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Хост базы данных
        /// </summary>
        [Required]
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// Порт базы данных
        /// </summary>
        [Range(1, 65535)]
        public int Port { get; set; } = 5432;

        /// <summary>
        /// Имя базы данных
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Пароль (получается из секретов)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Получить строку подключения
        /// </summary>
        /// <returns>Строка подключения к PostgreSQL</returns>
        public string GetConnectionString()
        {
            return $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
        }

        /// <summary>
        /// Получить строку подключения без пароля (для логирования)
        /// </summary>
        /// <returns>Строка подключения без пароля</returns>
        public string GetConnectionStringForLogging()
        {
            return $"Host={Host};Port={Port};Database={Name};Username={Username};Password=...";
        }
    }
}
