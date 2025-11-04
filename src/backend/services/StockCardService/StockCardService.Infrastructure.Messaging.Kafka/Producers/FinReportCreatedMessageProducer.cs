using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.Infrastructure.Messaging.Kafka
{
    /// <summary>
    ///  Реализация Kafka-продюсера для отправки сообщений о публикации финансового отчета
    /// </summary>
    //public class FinReportCreatedMessageProducer : IKafkaProducer<string, FinancialReportCreatedMessage>
    public class FinReportCreatedMessageProducer : IKafkaProducer<string, FinancialReportCreatedMessage>
    {
        /// <summary>
        /// Название Kafka-топика, в который публикуются сообщения.
        /// </summary>
        private const string TopicName = "created_financial_reports";

        /// <summary>
        /// Экземпляр Kafka-продюсера, созданный через фабрику.
        /// </summary>
        private readonly IProducer<string, FinancialReportCreatedMessage> _producer;

        /// <summary>
        /// Логгер для регистрации событий и ошибок.
        /// </summary>
        private readonly ILogger<FinReportCreatedMessageProducer> _logger;

        /// <summary>
        /// Конструктор продюсера сообщений о создании финансового отчёта.
        /// </summary>
        /// <param name="producerFactory">Фабрика Kafka-продюсеров.</param>
        /// <param name="logger">Логгер.</param>
        public FinReportCreatedMessageProducer(
            IKafkaProducerFactory producerFactory,
            ILogger<FinReportCreatedMessageProducer> logger)
        {
            _producer = producerFactory.Create<string, FinancialReportCreatedMessage>();
            _logger = logger;
        }

        /// <summary>
        /// Отправляет сообщение в прописанный топик.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public async Task ProduceAsync(FinancialReportCreatedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await _producer.ProduceAsync(TopicName, new Message<string, FinancialReportCreatedMessage>
                {
                    Key = message.ShareCardId.ToString(),
                    Value = message
                }, cancellationToken);

                _logger.LogInformation("Kafka: сообщение для ShareCardId={ShareCardId} отправлено", message.ShareCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka: ошибка при отправке сообщения для ShareCardId={ShareCardId}", message.ShareCardId);
                throw;
            }
        }

        public async Task ProduceAsync(int partitionId, FinancialReportCreatedMessage message, CancellationToken cancellationToken)
        {
            var partition = new TopicPartition(TopicName, new Partition(partitionId));
            try
            {
                await _producer.ProduceAsync(partition, new Message<string, FinancialReportCreatedMessage>
                {
                    Key = message.ShareCardId.ToString(),
                    Value = message
                }, cancellationToken);
                _logger?.LogInformation($"Message financial report with shareCardId {message.ShareCardId.ToString()} sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka: ошибка при отправке сообщения для ShareCardId={ShareCardId}", message.ShareCardId);
                throw;
            }
            
        }
    }
}
