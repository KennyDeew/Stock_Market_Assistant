using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.WebApi.Configuration;

namespace StockMarketAssistant.AnalyticsService.WebApi.Services
{
    /// <summary>
    /// Сервис для работы с секретами и конфигурацией
    /// </summary>
    public class SecretsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecretsService> _logger;

        public SecretsService(IConfiguration configuration, ILogger<SecretsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Получить конфигурацию базы данных с секретами
        /// </summary>
        /// <returns>Конфигурация базы данных</returns>
        public DatabaseConfiguration GetDatabaseConfiguration()
        {
            var dbConfig = new DatabaseConfiguration();
            _configuration.GetSection("Database").Bind(dbConfig);

            // Приоритет: секреты > переменные окружения > appsettings
            var password = GetSecret("Database:Password") ?? 
                          Environment.GetEnvironmentVariable("ANALYTICS_DB_PASSWORD") ?? 
                          dbConfig.Password;

            dbConfig.Password = password;

            _logger.LogInformation("Конфигурация базы данных загружена: {ConnectionString}", 
                dbConfig.GetConnectionStringForLogging());

            return dbConfig;
        }

        /// <summary>
        /// Получить конфигурацию Kafka с секретами
        /// </summary>
        /// <returns>Конфигурация Kafka</returns>
        public KafkaConfiguration GetKafkaConfiguration()
        {
            var kafkaConfig = new KafkaConfiguration();
            _configuration.GetSection("Kafka").Bind(kafkaConfig);

            // Приоритет: секреты > переменные окружения > appsettings
            var username = GetSecret("Kafka:SaslUsername") ?? 
                          Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME") ?? 
                          kafkaConfig.SaslUsername;

            var password = GetSecret("Kafka:SaslPassword") ?? 
                          Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD") ?? 
                          kafkaConfig.SaslPassword;

            kafkaConfig.SaslUsername = username;
            kafkaConfig.SaslPassword = password;

            _logger.LogInformation("Конфигурация Kafka загружена: {BootstrapServers}, SASL: {SaslEnabled}", 
                kafkaConfig.BootstrapServers, kafkaConfig.IsSaslEnabled());

            return kafkaConfig;
        }

        /// <summary>
        /// Получить конфигурацию Redis с секретами
        /// </summary>
        /// <returns>Конфигурация Redis</returns>
        public RedisConfiguration GetRedisConfiguration()
        {
            var redisConfig = new RedisConfiguration();
            _configuration.GetSection("Redis").Bind(redisConfig);

            // Приоритет: секреты > переменные окружения > appsettings
            var password = GetSecret("Redis:Password") ?? 
                          Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? 
                          redisConfig.Password;

            redisConfig.Password = password;

            _logger.LogInformation("Конфигурация Redis загружена: {ConnectionString}", 
                redisConfig.GetConnectionStringForLogging());

            return redisConfig;
        }

        /// <summary>
        /// Получить секрет по ключу
        /// </summary>
        /// <param name="key">Ключ секрета</param>
        /// <returns>Значение секрета или null</returns>
        private string? GetSecret(string key)
        {
            try
            {
                var secret = _configuration[key];
                if (!string.IsNullOrEmpty(secret))
                {
                    _logger.LogDebug("Секрет найден для ключа: {Key}", key);
                    return secret;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при получении секрета для ключа: {Key}", key);
            }

            return null;
        }

        /// <summary>
        /// Получить API ключ
        /// </summary>
        /// <param name="keyName">Имя ключа</param>
        /// <returns>API ключ или null</returns>
        public string? GetApiKey(string keyName)
        {
            var apiKey = GetSecret($"ApiKeys:{keyName}") ?? 
                        Environment.GetEnvironmentVariable($"API_KEY_{keyName.ToUpper()}");

            if (!string.IsNullOrEmpty(apiKey))
            {
                _logger.LogDebug("API ключ найден для: {KeyName}", keyName);
            }
            else
            {
                _logger.LogWarning("API ключ не найден для: {KeyName}", keyName);
            }

            return apiKey;
        }

        /// <summary>
        /// Проверить, что все необходимые секреты загружены
        /// </summary>
        /// <returns>True, если все секреты загружены</returns>
        public bool ValidateSecrets()
        {
            var dbConfig = GetDatabaseConfiguration();
            var kafkaConfig = GetKafkaConfiguration();
            var redisConfig = GetRedisConfiguration();

            var isValid = true;

            // Проверка базы данных
            if (string.IsNullOrEmpty(dbConfig.Password))
            {
                _logger.LogWarning("Пароль базы данных не настроен");
                isValid = false;
            }

            // Проверка Kafka (если SASL включен)
            if (kafkaConfig.IsSaslEnabled())
            {
                if (string.IsNullOrEmpty(kafkaConfig.SaslUsername) || string.IsNullOrEmpty(kafkaConfig.SaslPassword))
                {
                    _logger.LogWarning("Kafka SASL учетные данные не настроены");
                    isValid = false;
                }
            }

            // Проверка Redis (если требуется пароль)
            if (string.IsNullOrEmpty(redisConfig.Password))
            {
                _logger.LogInformation("Пароль Redis не настроен (используется анонимное подключение)");
            }

            if (isValid)
            {
                _logger.LogInformation("Все необходимые секреты загружены успешно");
            }
            else
            {
                _logger.LogWarning("Некоторые секреты не настроены");
            }

            return isValid;
        }
    }
}
