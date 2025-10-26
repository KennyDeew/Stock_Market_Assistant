using Confluent.Kafka;
using StockMarketAssistant.StockCardService.Domain.Interfaces;
using System.Text.Json;

namespace StockCardService.Infrastructure.Messaging.Kafka.Serializers
{
    public class KafkaMessageSerializer<T> : ISerializer<T> where T : IKafkaFinancialReport<Guid>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            using (var ms = new MemoryStream())
            {
                string jsonString = JsonSerializer.Serialize(data);
                var writer = new StreamWriter(ms);

                writer.Write(jsonString);
                writer.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }
    }
}
