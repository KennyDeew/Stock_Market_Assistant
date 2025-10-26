namespace StockCardService.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// Интерфейс для Kafka-продюсера
    /// </summary>
    public interface IKafkaProducer
    {
        /// <summary>
        /// Публикация сообщения в Kafka
        /// </summary>
        /// <typeparam name="T">Тип сообщения</typeparam>
        /// <param name="topic">Имя топика в Kafka</param>
        /// <param name="message">Экземпляр сообщения</param>
        /// <returns></returns>
        Task ProduceAsync<T>(string topic, T message);
    }
}
