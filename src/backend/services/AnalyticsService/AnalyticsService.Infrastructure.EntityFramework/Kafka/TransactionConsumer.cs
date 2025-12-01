using System.Linq;
using System.Net.Sockets;
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
            _logger.LogInformation("Запуск Kafka Consumer для топика: {Topic}, BootstrapServers: {BootstrapServers}",
                _config.Topic, _config.BootstrapServers);

            // Проверка доступности Kafka перед подпиской
            if (!await CheckKafkaAvailabilityAsync(_config.BootstrapServers, stoppingToken))
            {
                _logger.LogError(
                    "Kafka недоступен на {BootstrapServers}. Consumer будет остановлен. " +
                    "Убедитесь, что Kafka запущен и доступен. " +
                    "Проверьте: docker ps | findstr kafka или используйте скрипт: .\\scripts\\start_analytics_service.ps1",
                    _config.BootstrapServers);
                return;
            }

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
                _logger.LogInformation("Начало получения сообщений из Kafka. Топик: {Topic}, ConsumerGroup: {Group}",
                    _config.Topic, _config.ConsumerGroup);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // Собираем батч сообщений
                        var batch = new List<ConsumeResult<string, string>>();
                        var batchStartTime = DateTime.UtcNow;
                        var batchTimeout = TimeSpan.FromSeconds(5); // Таймаут для батча

                        // Собираем сообщения до достижения размера батча или таймаута
                        while (batch.Count < _config.BatchSize && !stoppingToken.IsCancellationRequested)
                        {
                            // Увеличиваем таймаут Consume для более надежного получения сообщений
                            var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(3000));

                            if (consumeResult != null)
                            {
                                // Проверяем, не является ли это служебным сообщением (например, IsPartitionEOF)
                                if (consumeResult.IsPartitionEOF)
                                {
                                    _logger.LogDebug("Достигнут конец партиции {Partition}. Offset: {Offset}",
                                        consumeResult.Partition, consumeResult.Offset);
                                    // Продолжаем получать сообщения
                                    continue;
                                }

                                // Проверяем наличие Message перед добавлением в батч
                                if (consumeResult.Message != null && !string.IsNullOrEmpty(consumeResult.Message.Value))
                                {
                                    batch.Add(consumeResult);
                                    _logger.LogDebug("Получено сообщение из Kafka. Offset: {Offset}, Partition: {Partition}",
                                        consumeResult.Offset, consumeResult.Partition);
                                }
                                else
                                {
                                    // Это может быть служебное сообщение или сообщение с пустым значением
                                    // Логируем подробную информацию для диагностики
                                    var messageInfo = new
                                    {
                                        Topic = consumeResult.Topic,
                                        Partition = consumeResult.Partition.ToString(),
                                        Offset = consumeResult.Offset.ToString(),
                                        IsPartitionEOF = consumeResult.IsPartitionEOF,
                                        MessageIsNull = consumeResult.Message == null,
                                        MessageValueIsNull = consumeResult.Message?.Value == null,
                                        MessageValueIsEmpty = string.IsNullOrEmpty(consumeResult.Message?.Value),
                                        MessageKey = consumeResult.Message?.Key ?? "null",
                                        Timestamp = consumeResult.Message?.Timestamp.UtcDateTime.ToString("O") ?? consumeResult.Timestamp.UtcDateTime.ToString("O")
                                    };

                                    _logger.LogWarning(
                                        "Пропущено сообщение с null или пустым значением. " +
                                        "Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, " +
                                        "IsPartitionEOF: {IsPartitionEOF}, MessageIsNull: {MessageIsNull}, " +
                                        "MessageValueIsNull: {MessageValueIsNull}, MessageValueIsEmpty: {MessageValueIsEmpty}, " +
                                        "MessageKey: {MessageKey}, Timestamp: {Timestamp}",
                                        messageInfo.Topic,
                                        messageInfo.Partition,
                                        messageInfo.Offset,
                                        messageInfo.IsPartitionEOF,
                                        messageInfo.MessageIsNull,
                                        messageInfo.MessageValueIsNull,
                                        messageInfo.MessageValueIsEmpty,
                                        messageInfo.MessageKey,
                                        messageInfo.Timestamp);
                                }
                            }
                            else
                            {
                                // Если нет новых сообщений
                                if (batch.Count > 0)
                                {
                                    // Если есть сообщения в батче, но новых нет, обрабатываем батч
                                    _logger.LogDebug("Новых сообщений нет, обрабатываем накопленный батч из {Count} сообщений", batch.Count);
                                    break;
                                }
                                else
                                {
                                    // Если батч пуст и нет новых сообщений, делаем небольшую паузу
                                    // чтобы не нагружать CPU постоянными вызовами Consume
                                    await Task.Delay(100, stoppingToken);

                                    // Проверяем, не истек ли таймаут батча
                                    if (DateTime.UtcNow - batchStartTime > batchTimeout)
                                    {
                                        _logger.LogDebug("Таймаут батча истек, выходим из цикла сбора сообщений");
                                        break;
                                    }
                                }
                            }
                        }

                        if (batch.Count > 0)
                        {
                            _logger.LogInformation("Получен батч из {Count} сообщений. Обработка...", batch.Count);
                            await ProcessBatchAsync(batch, stoppingToken);
                        }
                        else
                        {
                            // Логируем только периодически, чтобы не засорять логи
                            if (DateTime.UtcNow.Second % 30 == 0) // Каждые 30 секунд
                            {
                                _logger.LogDebug("Ожидание сообщений из Kafka. Топик: {Topic}", _config.Topic);
                            }
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
                        else if (ex.Error?.Code == ErrorCode.Local_Resolve || ex.Error?.Code == ErrorCode.Local_AllBrokersDown)
                        {
                            _logger.LogError(
                                ex,
                                "Kafka недоступен на {BootstrapServers}. Ошибка: {Reason} (Code: {Code}). " +
                                "Проверьте, что Kafka запущен: docker ps | findstr kafka или используйте скрипт: .\\scripts\\start_analytics_service.ps1",
                                _config.BootstrapServers, ex.Error?.Reason, ex.Error?.Code);
                            // Ждем дольше перед повторной попыткой при проблемах с подключением
                            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                        }
                        else
                        {
                            _logger.LogError(ex, "Ошибка при потреблении сообщений из Kafka: {Reason} (Code: {Code})",
                                ex.Error?.Reason, ex.Error?.Code);
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
                        else if (ex.Error?.Code == ErrorCode.Local_Resolve || ex.Error?.Code == ErrorCode.Local_AllBrokersDown)
                        {
                            _logger.LogError(
                                ex,
                                "Kafka недоступен на {BootstrapServers}. Ошибка: {Error} (Code: {Code}). " +
                                "Проверьте, что Kafka запущен: docker ps | findstr kafka или используйте скрипт: .\\scripts\\start_analytics_service.ps1",
                                _config.BootstrapServers, ex.Message, ex.Error?.Code);
                            // Ждем дольше перед повторной попыткой при проблемах с подключением
                            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                        }
                        else
                        {
                            _logger.LogError(ex, "Kafka ошибка: {Error} (Code: {Code})", ex.Message, ex.Error?.Code);
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
                    // Проверка на null
                    if (consumeResult == null)
                    {
                        _logger.LogWarning("ConsumeResult равен null, пропускаем сообщение");
                        continue;
                    }

                    if (consumeResult.Message == null)
                    {
                        // Подробное логирование сообщения с null Message
                        var nullMessageInfo = new
                        {
                            Topic = consumeResult.Topic,
                            Partition = consumeResult.Partition.ToString(),
                            Offset = consumeResult.Offset.ToString(),
                            IsPartitionEOF = consumeResult.IsPartitionEOF,
                            Timestamp = consumeResult.Message?.Timestamp.UtcDateTime.ToString("O") ?? consumeResult.Timestamp.UtcDateTime.ToString("O"),
                            Headers = consumeResult.Message?.Headers != null
                                ? string.Join(", ", consumeResult.Message.Headers.Select(h => $"{h.Key}={System.Text.Encoding.UTF8.GetString(h.GetValueBytes())}"))
                                : "null"
                        };

                        _logger.LogWarning(
                            "Message равен null в ConsumeResult, пропускаем сообщение. " +
                            "Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, " +
                            "IsPartitionEOF: {IsPartitionEOF}, Timestamp: {Timestamp}, Headers: {Headers}",
                            nullMessageInfo.Topic,
                            nullMessageInfo.Partition,
                            nullMessageInfo.Offset,
                            nullMessageInfo.IsPartitionEOF,
                            nullMessageInfo.Timestamp,
                            nullMessageInfo.Headers);
                        continue;
                    }

                    if (string.IsNullOrEmpty(consumeResult.Message.Value))
                    {
                        _logger.LogWarning("Message.Value пуст в ConsumeResult, пропускаем сообщение. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                            consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                        continue;
                    }

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
                    // Безопасное логирование с проверкой на null
                    var messageValue = consumeResult?.Message?.Value ?? "null";
                    _logger.LogError(ex, "Ошибка при десериализации/маппинге сообщения: {Value}", messageValue);
                    if (consumeResult != null)
                    {
                        failedMessages.Add((consumeResult, ex));
                    }
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

        /// <summary>
        /// Проверка доступности Kafka перед подпиской
        /// </summary>
        private async Task<bool> CheckKafkaAvailabilityAsync(string bootstrapServers, CancellationToken cancellationToken)
        {
            try
            {
                // Парсим адрес и порт
                var parts = bootstrapServers.Split(':');
                if (parts.Length != 2)
                {
                    _logger.LogWarning("Некорректный формат BootstrapServers: {BootstrapServers}. Ожидается формат: host:port", bootstrapServers);
                    return false;
                }

                var host = parts[0];
                if (!int.TryParse(parts[1], out var port))
                {
                    _logger.LogWarning("Некорректный порт в BootstrapServers: {BootstrapServers}", bootstrapServers);
                    return false;
                }

                // Проверяем доступность через TCP подключение
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == connectTask && tcpClient.Connected)
                {
                    tcpClient.Close();
                    _logger.LogInformation("Kafka доступен на {BootstrapServers}", bootstrapServers);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Kafka недоступен на {BootstrapServers} (таймаут подключения)", bootstrapServers);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка при проверке доступности Kafka на {BootstrapServers}", bootstrapServers);
                return false;
            }
        }
    }
}

