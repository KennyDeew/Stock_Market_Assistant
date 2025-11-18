using Confluent.Kafka;
using System.Text.Json;

namespace StockCardService.Infrastructure.Messaging.Kafka.Serializers
{
    /// <summary>
    /// Сериализатор сообщений Kafka в формат JSON
    /// </summary>
    /// <typeparam name="T">Тип сообщения</typeparam>
    public class KafkaMessageSerializer<T> : ISerializer<T>
    {
        /// <summary>
        /// Сериализует объект "сообщение" в байты JSON для отправки в Kafka.
        /// </summary>
        /// <param name="data">Объект (сообщение)</param>
        /// <param name="context">Контекст сериализации (Kafka API).</param>
        /// <returns>Байтовый массив в формате JSON.</returns>
        public byte[] Serialize(T data, SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
