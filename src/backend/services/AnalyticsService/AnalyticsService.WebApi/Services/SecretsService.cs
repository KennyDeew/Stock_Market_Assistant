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

            // Приоритет: Docker секреты > переменные окружения > appsettings
            var password = GetSecret("Database:Password") ??
                          Environment.GetEnvironmentVariable("ANALYTICS_DB_PASSWORD") ??
                          Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ??
                          dbConfig.Password;

            // Получаем настройки из переменных окружения Docker
            var host = Environment.GetEnvironmentVariable("ANALYTICS_DB_HOST") ??
                      Environment.GetEnvironmentVariable("POSTGRES_HOST") ??
                      dbConfig.Host;

            var port = int.TryParse(Environment.GetEnvironmentVariable("ANALYTICS_DB_PORT") ??
                                   Environment.GetEnvironmentVariable("POSTGRES_PORT"), out var envPort)
                      ? envPort : dbConfig.Port;

            var name = Environment.GetEnvironmentVariable("ANALYTICS_DB_NAME") ??
                      Environment.GetEnvironmentVariable("POSTGRES_DB") ??
                      dbConfig.Name;

            var username = Environment.GetEnvironmentVariable("ANALYTICS_DB_USERNAME") ??
                          Environment.GetEnvironmentVariable("POSTGRES_USER") ??
                          dbConfig.Username;

            dbConfig.Host = host;
            dbConfig.Port = port;
            dbConfig.Name = name;
            dbConfig.Username = username;
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

            // Приоритет: Docker секреты > переменные окружения > appsettings
            var username = GetSecret("Kafka:SaslUsername") ??
                          Environment.GetEnvironmentVariable("KAFKA_SASL_USERNAME") ??
                          Environment.GetEnvironmentVariable("KAFKA_USERNAME") ??
                          kafkaConfig.SaslUsername;

            var password = GetSecret("Kafka:SaslPassword") ??
                          Environment.GetEnvironmentVariable("KAFKA_SASL_PASSWORD") ??
                          Environment.GetEnvironmentVariable("KAFKA_PASSWORD") ??
                          kafkaConfig.SaslPassword;

            // Получаем настройки из переменных окружения Docker
            var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ??
                                  Environment.GetEnvironmentVariable("KAFKA_SERVERS") ??
                                  kafkaConfig.BootstrapServers;

            var groupId = Environment.GetEnvironmentVariable("KAFKA_GROUP_ID") ??
                         kafkaConfig.GroupId;

            var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ??
                       kafkaConfig.Topic;

            kafkaConfig.BootstrapServers = bootstrapServers;
            kafkaConfig.GroupId = groupId;
            kafkaConfig.Topic = topic;
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

            // Приоритет: Docker секреты > переменные окружения > appsettings
            var password = GetSecret("Redis:Password") ??
                          Environment.GetEnvironmentVariable("REDIS_PASSWORD") ??
                          Environment.GetEnvironmentVariable("REDIS_AUTH") ??
                          redisConfig.Password;

            // Получаем настройки из переменных окружения Docker
            var host = Environment.GetEnvironmentVariable("REDIS_HOST") ??
                      Environment.GetEnvironmentVariable("REDIS_SERVER") ??
                      redisConfig.Host;

            var port = int.TryParse(Environment.GetEnvironmentVariable("REDIS_PORT"), out var envPort)
                      ? envPort : redisConfig.Port;

            var database = int.TryParse(Environment.GetEnvironmentVariable("REDIS_DATABASE"), out var envDatabase)
                          ? envDatabase : redisConfig.Database;

            redisConfig.Host = host;
            redisConfig.Port = port;
            redisConfig.Database = database;
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
                // Приоритет 1: Docker секреты (файлы в /run/secrets/)
                var dockerSecretPath = $"/run/secrets/{key.Replace(":", "_").ToLower()}";
                if (File.Exists(dockerSecretPath))
                {
                    var secret = File.ReadAllText(dockerSecretPath).Trim();
                    if (!string.IsNullOrEmpty(secret))
                    {
                        _logger.LogDebug("Docker секрет найден для ключа: {Key}", key);
                        return secret;
                    }
                }

                // Приоритет 2: Переменные окружения Docker
                var envKey = key.Replace(":", "_").ToUpper();
                var envValue = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrEmpty(envValue))
                {
                    _logger.LogDebug("Переменная окружения найдена для ключа: {Key}", key);
                    return envValue;
                }

                // Приоритет 3: Конфигурация приложения
                var configValue = _configuration[key];
                if (!string.IsNullOrEmpty(configValue))
                {
                    _logger.LogDebug("Конфигурация найдена для ключа: {Key}", key);
                    return configValue;
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
