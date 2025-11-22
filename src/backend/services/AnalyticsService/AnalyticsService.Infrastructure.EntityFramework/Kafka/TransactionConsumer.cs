using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Events;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Kafka
{
    /// <summary>
    /// Background Service для потребления транзакций из Kafka
    /// </summary>
    public class TransactionConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly KafkaConfiguration _config;
        private readonly ILogger<TransactionConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProducer<string, string>? _dlqProducer;
        private readonly IEventBus? _eventBus;

        public TransactionConsumer(
            IConsumer<string, string> consumer,
            IOptions<KafkaConfiguration> config,
            ILogger<TransactionConsumer> logger,
            IServiceProvider serviceProvider,
            IProducer<string, string>? dlqProducer = null,
            IEventBus? eventBus = null)
        {
            _consumer = consumer;
            _config = config.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _dlqProducer = dlqProducer;
            _eventBus = eventBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Запуск Kafka Consumer для топика: {Topic}", _config.Topic);
            _consumer.Subscribe(_config.Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Собираем батч сообщений
                        var batch = new List<ConsumeResult<string, string>>();
                        var batchStartTime = DateTime.UtcNow;

                        // Собираем сообщения до достижения размера батча или таймаута
                        while (batch.Count < _config.BatchSize && !stoppingToken.IsCancellationRequested)
                        {
                            var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(1000));

                            if (consumeResult != null)
                            {
                                batch.Add(consumeResult);
                            }
                            else if (batch.Count > 0)
                            {
                                // Если есть сообщения в батче, но новых нет, обрабатываем батч
                                break;
                            }
                        }

                        if (batch.Count > 0)
                        {
                            _logger.LogInformation("Получен батч из {Count} сообщений", batch.Count);
                            await ProcessBatchAsync(batch, stoppingToken);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Ошибка при потреблении сообщений из Kafka");
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Операция отменена");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Неожиданная ошибка в Kafka Consumer");
                        await Task.Delay(5000, stoppingToken);
                    }
                }
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Kafka Consumer остановлен");
            }
        }

        /// <summary>
        /// Обработка батча сообщений
        /// </summary>
        private async Task ProcessBatchAsync(
            List<ConsumeResult<string, string>> batch,
            CancellationToken cancellationToken)
        {
            var processedMessages = new List<ConsumeResult<string, string>>();
            var failedMessages = new List<(ConsumeResult<string, string> Message, Exception Error)>();

            using var scope = _serviceProvider.CreateScope();
            var transactionRepository = scope.ServiceProvider.GetRequiredService<IAssetTransactionRepository>();

            foreach (var consumeResult in batch)
            {
                try
                {
                    var message = JsonSerializer.Deserialize<TransactionMessage>(consumeResult.Message.Value);

                    if (message == null)
                    {
                        _logger.LogWarning("Не удалось десериализовать сообщение: {Value}", consumeResult.Message.Value);
                        failedMessages.Add((consumeResult, new InvalidOperationException("Не удалось десериализовать сообщение")));
                        continue;
                    }

                    // Преобразуем TransactionMessage в AssetTransaction
                    var assetTransaction = MapToAssetTransaction(message);

                    // Сохраняем транзакцию в базу данных
                    await transactionRepository.AddAsync(assetTransaction, cancellationToken);
                    await transactionRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Транзакция сохранена: PortfolioId={PortfolioId}, StockCardId={StockCardId}, Type={Type}, Quantity={Quantity}",
                        assetTransaction.PortfolioId,
                        assetTransaction.StockCardId,
                        assetTransaction.TransactionType,
                        assetTransaction.Quantity);

                    // Публикация события TransactionReceivedEvent после сохранения
                    if (_eventBus != null)
                    {
                        var transactionEvent = new TransactionReceivedEvent(assetTransaction);
                        await _eventBus.PublishAsync(transactionEvent, cancellationToken);
                        _logger.LogInformation(
                            "Событие TransactionReceivedEvent опубликовано: TransactionId={TransactionId}",
                            assetTransaction.Id);
                    }
                    else
                    {
                        _logger.LogWarning("EventBus не настроен, событие TransactionReceivedEvent не будет опубликовано");
                    }

                    processedMessages.Add(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщения: {Value}", consumeResult.Message.Value);
                    failedMessages.Add((consumeResult, ex));
                }
            }

            // Обработка неудачных сообщений (Dead Letter Queue)
            if (failedMessages.Count > 0)
            {
                await HandleFailedMessagesAsync(failedMessages, cancellationToken);
            }

            // Коммитим только успешно обработанные сообщения
            if (processedMessages.Count > 0)
            {
                try
                {
                    // Коммитим последнее сообщение в батче (все предыдущие будут закоммичены автоматически)
                    var lastMessage = processedMessages.Last();
                    _consumer.Commit(lastMessage);
                    _logger.LogInformation("Успешно обработано и закоммичено {Count} сообщений", processedMessages.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при коммите сообщений");
                }
            }
        }

        /// <summary>
        /// Преобразование TransactionMessage в AssetTransaction
        /// </summary>
        private AssetTransaction MapToAssetTransaction(TransactionMessage message)
        {
            var transactionType = message.TransactionType == 1
                ? TransactionType.Buy
                : TransactionType.Sell;

            var assetType = (AssetType)message.AssetType;

            if (transactionType == TransactionType.Buy)
            {
                return AssetTransaction.CreateBuyTransaction(
                    message.PortfolioId,
                    message.StockCardId,
                    assetType,
                    message.Quantity,
                    message.PricePerUnit,
                    message.TransactionTime,
                    message.Currency,
                    message.Metadata);
            }
            else
            {
                return AssetTransaction.CreateSellTransaction(
                    message.PortfolioId,
                    message.StockCardId,
                    assetType,
                    message.Quantity,
                    message.PricePerUnit,
                    message.TransactionTime,
                    message.Currency,
                    message.Metadata);
            }
        }

        /// <summary>
        /// Обработка неудачных сообщений (Dead Letter Queue)
        /// </summary>
        private async Task HandleFailedMessagesAsync(
            List<(ConsumeResult<string, string> Message, Exception Error)> failedMessages,
            CancellationToken cancellationToken)
        {
            if (_dlqProducer == null)
            {
                _logger.LogWarning("DLQ Producer не настроен, неудачные сообщения не будут отправлены в DLQ");
                return;
            }

            foreach (var (message, error) in failedMessages)
            {
                try
                {
                    var dlqMessage = new Message<string, string>
                    {
                        Key = message.Message.Key,
                        Value = message.Message.Value,
                        Headers = new Headers
                        {
                            { "original-topic", System.Text.Encoding.UTF8.GetBytes(message.Topic) },
                            { "error-message", System.Text.Encoding.UTF8.GetBytes(error.Message) },
                            { "error-type", System.Text.Encoding.UTF8.GetBytes(error.GetType().Name) },
                            { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                        }
                    };

                    await _dlqProducer.ProduceAsync(
                        $"{_config.Topic}.dlq",
                        dlqMessage,
                        cancellationToken);

                    _logger.LogInformation("Сообщение отправлено в DLQ: {Topic}", $"{_config.Topic}.dlq");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке сообщения в DLQ");
                }
            }
        }
    }
}

