using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Events;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using KafkaException = Confluent.Kafka.KafkaException;

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

            // Пытаемся подписаться на топик с повторными попытками
            int retryCount = 0;
            const int maxRetries = 10;
            const int retryDelaySeconds = 5;

            while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _consumer.Subscribe(_config.Topic);
                    _logger.LogInformation("Успешно подписались на топик {Topic}", _config.Topic);
                    break; // Успешно подписались, выходим из цикла
                }
                catch (KafkaException kex)
                {
                    retryCount++;
                    if (kex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning(
                            "Топик {Topic} не существует (попытка {Retry}/{MaxRetries}). Ожидание создания топика...",
                            _config.Topic, retryCount, maxRetries);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Ошибка при подписке на топик {Topic} (попытка {Retry}/{MaxRetries}): {Error}",
                            _config.Topic, retryCount, maxRetries, kex.Error.Reason);
                    }

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(
                            "Не удалось подписаться на топик {Topic} после {MaxRetries} попыток. Consumer будет остановлен. Убедитесь, что топик создан.",
                            _config.Topic, maxRetries);
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), stoppingToken);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(
                        "Ошибка при подписке на топик {Topic} (попытка {Retry}/{MaxRetries}): {Error}",
                        _config.Topic, retryCount, maxRetries, ex.Message);

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(ex, "Не удалось подписаться на топик {Topic} после {MaxRetries} попыток. Consumer будет остановлен.", _config.Topic, maxRetries);
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), stoppingToken);
                }
            }

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
                        if (ex.Error?.Code == ErrorCode.UnknownTopicOrPart)
                        {
                            _logger.LogWarning(
                                "Топик {Topic} недоступен. Ожидание создания топика... (попытка переподключения через 10 секунд)",
                                _config.Topic);
                            // Пытаемся переподписаться
                            try
                            {
                                _consumer.Unsubscribe();
                                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                                _consumer.Subscribe(_config.Topic);
                                _logger.LogInformation("Переподписка на топик {Topic} выполнена", _config.Topic);
                            }
                            catch (Exception rex)
                            {
                                _logger.LogWarning(rex, "Не удалось переподписаться на топик {Topic}", _config.Topic);
                                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                            }
                        }
                        else
                        {
                            _logger.LogError(ex, "Ошибка при потреблении сообщений из Kafka: {Reason}", ex.Error?.Reason);
                            await Task.Delay(5000, stoppingToken);
                        }
                    }
                    catch (KafkaException ex)
                    {
                        if (ex.Error?.Code == ErrorCode.UnknownTopicOrPart)
                        {
                            _logger.LogWarning(
                                "Топик {Topic} недоступен. Ожидание создания топика... (попытка переподключения через 10 секунд)",
                                _config.Topic);
                            try
                            {
                                _consumer.Unsubscribe();
                                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                                _consumer.Subscribe(_config.Topic);
                                _logger.LogInformation("Переподписка на топик {Topic} выполнена", _config.Topic);
                            }
                            catch (Exception rex)
                            {
                                _logger.LogWarning(rex, "Не удалось переподписаться на топик {Topic}", _config.Topic);
                                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                            }
                        }
                        else
                        {
                            _logger.LogError(ex, "Kafka ошибка: {Error}", ex.Message);
                            await Task.Delay(5000, stoppingToken);
                        }
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
                try
                {
                    _consumer.Close();
                    _logger.LogInformation("Kafka Consumer остановлен");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка при закрытии Kafka Consumer");
                }
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
            var transactionsToSave = new List<(ConsumeResult<string, string> Message, AssetTransaction Transaction)>();

            using var scope = _serviceProvider.CreateScope();
            var transactionRepository = scope.ServiceProvider.GetRequiredService<IAssetTransactionRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            // Шаг 1: Десериализация и валидация всех сообщений
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
                    transactionsToSave.Add((consumeResult, assetTransaction));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при десериализации/маппинге сообщения: {Value}", consumeResult.Message.Value);
                    failedMessages.Add((consumeResult, ex));
                }
            }

            // Шаг 2: Батчевое сохранение всех транзакций в одной транзакции БД
            if (transactionsToSave.Count > 0)
            {
                try
                {
                    // Используем транзакцию БД для атомарности
                    using var dbTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                    try
                    {
                        // Собираем все транзакции для батчевого сохранения
                        var transactions = transactionsToSave.Select(t => t.Transaction).ToList();

                        // Сохраняем все транзакции одним запросом
                        await transactionRepository.AddRangeAsync(transactions, cancellationToken);
                        var savedCount = await transactionRepository.SaveChangesAsync(cancellationToken);

                        // Коммитим транзакцию БД
                        await dbTransaction.CommitAsync(cancellationToken);

                        _logger.LogInformation(
                            "Батч транзакций сохранен: {Count} транзакций за один запрос к БД",
                            savedCount);

                        // Шаг 3: Публикация событий для всех успешно сохраненных транзакций
                        if (_eventBus != null)
                        {
                            foreach (var (message, transaction) in transactionsToSave)
                            {
                                try
                                {
                                    var transactionEvent = new TransactionReceivedEvent(transaction);
                                    await _eventBus.PublishAsync(transactionEvent, cancellationToken);

                                    _logger.LogDebug(
                                        "Событие TransactionReceivedEvent опубликовано: TransactionId={TransactionId}, PortfolioId={PortfolioId}, StockCardId={StockCardId}",
                                        transaction.Id,
                                        transaction.PortfolioId,
                                        transaction.StockCardId);
                                }
                                catch (Exception ex)
                                {
                                    // Логируем ошибку публикации события, но не прерываем обработку
                                    _logger.LogWarning(ex,
                                        "Ошибка при публикации события для транзакции {TransactionId}. Транзакция уже сохранена в БД.",
                                        transaction.Id);
                                }
                            }

                            _logger.LogInformation(
                                "Опубликовано событий TransactionReceivedEvent: {Count}",
                                transactionsToSave.Count);
                        }
                        else
                        {
                            _logger.LogWarning("EventBus не настроен, события TransactionReceivedEvent не будут опубликованы");
                        }

                        // Все сообщения успешно обработаны
                        processedMessages.AddRange(transactionsToSave.Select(t => t.Message));
                    }
                    catch (Exception ex)
                    {
                        // Откатываем транзакцию БД при ошибке
                        await dbTransaction.RollbackAsync(cancellationToken);
                        _logger.LogError(ex, "Ошибка при сохранении батча транзакций. Транзакция БД откачена.");

                        // Все сообщения из батча считаются неудачными
                        foreach (var (message, _) in transactionsToSave)
                        {
                            failedMessages.Add((message, ex));
                        }
                        transactionsToSave.Clear();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критическая ошибка при обработке батча транзакций");
                    // Все сообщения из батча считаются неудачными
                    foreach (var (message, _) in transactionsToSave)
                    {
                        failedMessages.Add((message, ex));
                    }
                    transactionsToSave.Clear();
                }
            }

            // Шаг 4: Обработка неудачных сообщений (Dead Letter Queue)
            if (failedMessages.Count > 0)
            {
                _logger.LogWarning("Обнаружено {Count} неудачных сообщений из батча {BatchSize}", failedMessages.Count, batch.Count);
                await HandleFailedMessagesAsync(failedMessages, cancellationToken);
            }

            // Шаг 5: Коммитим только успешно обработанные сообщения в Kafka
            if (processedMessages.Count > 0)
            {
                try
                {
                    // Коммитим последнее сообщение в батче (все предыдущие будут закоммичены автоматически)
                    var lastMessage = processedMessages.Last();
                    _consumer.Commit(lastMessage);
                    _logger.LogInformation(
                        "Успешно обработано и закоммичено {ProcessedCount} из {TotalCount} сообщений в Kafka",
                        processedMessages.Count,
                        batch.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при коммите сообщений в Kafka. Сообщения могут быть обработаны повторно.");
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

