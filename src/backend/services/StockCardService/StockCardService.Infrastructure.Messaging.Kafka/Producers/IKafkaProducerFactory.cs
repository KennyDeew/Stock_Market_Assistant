using Confluent.Kafka;

namespace StockCardService.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// интерфейс фабрики продюсера
    /// </summary>
    public interface IKafkaProducerFactory
    {
        /// <summary>
        /// создание кафка продюсера
        /// </summary>
        /// <typeparam name="TKey">Тип ключа Kafka-сообщения.</typeparam>
        /// <typeparam name="TValue">Тип значения Kafka-сообщения.</typeparam>
        /// <returns>кафка продюсер</returns>
        IProducer<TKey, TValue> Create<TKey, TValue>();
    }
}
