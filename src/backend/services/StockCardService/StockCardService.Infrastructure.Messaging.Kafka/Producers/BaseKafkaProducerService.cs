using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using StockCardService.Infrastructure.Messaging.Kafka.Options;
using StockCardService.Infrastructure.Messaging.Kafka.Serializers;
using StockMarketAssistant.StockCardService.Domain.Interfaces;
using Partitioner = Confluent.Kafka.Partitioner;

namespace StockCardService.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// Базовый обобщённый (generic) продюсер Kafka,
    /// отвечающий за создание и конфигурацию IProducer с нужными сериализаторами и логированием.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class BaseProducer<TKey, TValue> where TValue : IKafkaFinancialReport<Guid>
    {
        /// <summary>
        /// Логгер
        /// </summary>
        protected readonly ILogger? Logger;

        /// <summary>
        /// Kafka-продюсер из библиотеки Confluent.Kafka.
        /// </summary>
        protected IProducer<TKey, TValue> Producer { get; }

        /// <summary>
        /// Конструктор, создающий Kafka-продюсер с заданной конфигурацией.
        /// </summary>
        /// <param name="kafkaOptions">Объект с настройками Kafka (BootstrapServers).</param>
        /// <param name="logger">Логгер</param>
        public BaseProducer(KafkaOptions kafkaOptions, ILogger logger)
        {
            // Конфигурация продюсера
            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = kafkaOptions.BootstrapServers, // Адрес брокеров Kafka
                //Partitioner = Partitioner.Random, // Раскомментируй, если хочешь случайное распределение сообщений по партициям
                Partitioner = Partitioner.Consistent, // Сообщения с одинаковым ключом будут попадать в одну и ту же партицию
            };

            // Создаём билдера продюсера с типами ключа и значения
            var producerBuilder = new ProducerBuilder<TKey, TValue>(producerConfig);

            // Конфигурируем продюсер (цепочка методов)
            Producer = producerBuilder
                // Логирование ошибок
                .SetErrorHandler((_, error) => Logger?.LogError(error.Reason))
                // Внутреннее логирование
                .SetLogHandler((_, message) => Logger?.LogInformation(message.Message))
                // Устанавливаем сериализатор для ключей сообщений
                .SetKeySerializer((ISerializer<TKey>)Confluent.Kafka.Serializers.Utf8)
                //.SetKeySerializer((ISerializer<TKey>)Confluent.Kafka.Serializers.Int64) // Альтернатива для ключей типа long
                // Устанавливаем сериализатор для значений (сообщений)
                .SetValueSerializer(new KafkaMessageSerializer<TValue>())
                .Build();
        }
    }
}
