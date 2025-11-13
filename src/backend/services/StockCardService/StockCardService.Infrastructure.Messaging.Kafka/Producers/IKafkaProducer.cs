namespace StockCardService.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// Интерфейс для Kafka-продюсера
    /// </summary>
    /// <typeparam name="TKey">Тип ключа Kafka-сообщения.</typeparam>
    /// <typeparam name="TValue">Тип значения Kafka-сообщения.</typeparam>
    public interface IKafkaProducer<TKey, TValue>
    {
        /// <summary>
        /// Публикация сообщения в Kafka
        /// </summary>
        /// <param name="message">Сообщение, которое нужно опубликовать.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task ProduceAsync(TValue message, CancellationToken cancellationToken);
    }
}
