using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockCardService.Infrastructure.Messaging.Kafka.Options;
using StockCardService.Infrastructure.Messaging.Kafka.Serializers;

namespace StockCardService.Infrastructure.Messaging.Kafka
{
    public class KafkaProducerFactory : IKafkaProducerFactory
    {
        /// <summary>
        /// Настройки Kafka, содержащие параметры подключения.
        /// </summary>
        private readonly KafkaOptions _options;

        /// <summary>
        /// Логгер для регистрации событий, ошибок и внутренней информации.
        /// </summary>
        private readonly ILogger<KafkaProducerFactory> _logger;

        /// <summary>
        /// Конструктор фабрики Kafka-продюсеров.
        /// </summary>
        /// <param name="options">Опции Kafka, внедрённые через IOptions и KafkaOptions.</param>
        /// <param name="logger">Логгер.</param>
        public KafkaProducerFactory(IOptions<KafkaOptions> options, ILogger<KafkaProducerFactory> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Создаёт новый экземпляр Kafka-продюсера с типами ключа и значения.
        /// </summary>
        /// <typeparam name="TKey">Тип ключа Kafka-сообщения.</typeparam>
        /// <typeparam name="TValue">Тип значения Kafka-сообщения.</typeparam>
        /// <returns>Объект <see cref="IProducer{TKey, TValue}"/>.</returns>
        public IProducer<TKey, TValue> Create<TKey, TValue>()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                Partitioner = Partitioner.Consistent // одинаковые ключи → одинаковая партиция
            };

            return new ProducerBuilder<TKey, TValue>(config)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka error: {e.Reason}"))
                .SetLogHandler((_, log) => _logger.LogInformation(log.Message))
                .SetValueSerializer(new KafkaMessageSerializer<TValue>())
                .Build();
        }
    }
}
