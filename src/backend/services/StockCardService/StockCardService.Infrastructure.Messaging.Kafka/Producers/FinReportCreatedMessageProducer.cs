using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockCardService.Infrastructure.Messaging.Kafka.Options;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.Infrastructure.Messaging.Kafka
{
    /// <summary>
    ///  Реализация Kafka-продюсера для отправки сообщений о публикации финансового отчета
    /// </summary>
    //public class FinReportCreatedMessageProducer : IKafkaProducer<string, FinancialReportCreatedMessage>
    public class FinReportCreatedMessageProducer : BaseProducer<string, FinancialReportCreatedMessage>
    {
        private const string TopicName = "created_financial_reports";

        public FinReportCreatedMessageProducer(
        IOptions<ApplicationOptions> appOptions,
        ILogger<FinReportCreatedMessageProducer> logger)
        : base(appOptions.Value.KafkaOptions, logger)
        {
        }

        //public FinReportCreatedMessageProducer(
        //    ApplicationOptions applicationOptions,
        //    ILogger logger) :
        //    base(applicationOptions.KafkaOptions, logger)
        //{

        //}

        public async Task ProduceAsync(FinancialReportCreatedMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await Producer.ProduceAsync(TopicName, new Message<string, FinancialReportCreatedMessage>
                {
                    Key = message.ShareCardId.ToString(),
                    Value = message
                }, cancellationToken);
                var loggedKey = message.ShareCardId.ToString() != null ? message.ShareCardId.ToString() : "null";
                Logger?.LogInformation($"Message for financial report with shareCardId {loggedKey} sent");
            }
            catch (Exception e)
            {
                Logger?.LogError(e.Message);
            }
        }

        public async Task ProduceAsync(int partitionId, FinancialReportCreatedMessage message, CancellationToken cancellationToken)
        {
            var partition = new TopicPartition(TopicName, new Partition(partitionId));
            await Producer.ProduceAsync(partition, new Message<string, FinancialReportCreatedMessage>
            {
                Key = message.ShareCardId.ToString(),
                Value = message
            }, cancellationToken);
            Logger?.LogInformation($"Message financial report with shareCardId {message.ShareCardId.ToString()} sent");
        }
    }
}
