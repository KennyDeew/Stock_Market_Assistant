using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Фоновый сервис для обработки исходящих сообщений Kafka
    /// </summary>
    /// <remarks>
    /// Конструктор процессора Outbox
    /// </remarks>
    /// <param name="serviceProvider">Провайдер сервисов</param>
    /// <param name="producer">Продюсер Kafka</param>
    /// <param name="logger">Логгер</param>
    /// <param name="kafkaSettings">Настройки Kafka</param>
    public class KafkaOutboxProcessor(
        IServiceProvider serviceProvider,
        IProducer<Null, string> producer,
        ILogger<KafkaOutboxProcessor> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IProducer<Null, string> _producer = producer;
        private readonly ILogger<KafkaOutboxProcessor> _logger = logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Процессор Kafka Outbox запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                        var messages = await outboxRepository.GetUnprocessedMessagesAsync(100, stoppingToken);

                        _logger.LogDebug("Найдено {Count} необработанных сообщений", messages.Count());

                        foreach (var message in messages)
                        {
                            if (stoppingToken.IsCancellationRequested)
                                break;

                            await ProcessMessageAsync(message, outboxRepository, stoppingToken);
                        }
                    }

                    _logger.LogDebug("Цикл обработки Outbox завершен");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Ошибка во время обработки Outbox");
                }

                await Task.Delay(_processingInterval, stoppingToken);
            }

            _logger.LogInformation("Процессор Kafka Outbox остановлен");
        }

        /// <summary>
        /// Обработать отдельное сообщение
        /// </summary>
        /// <param name="message">Сообщение для обработки</param>
        /// <param name="outboxRepository">Репозиторий Outbox</param>
        /// <param name="cancellationToken">Токен отмены</param>
        private async Task ProcessMessageAsync(
            OutboxMessage message,
            IOutboxRepository outboxRepository,
            CancellationToken cancellationToken)
        {
            try
            {
                var kafkaMessage = new Message<Null, string>
                {
                    Value = message.Message,
                    Timestamp = new Timestamp(DateTime.UtcNow)
                };

                var deliveryResult = await _producer.ProduceAsync(
                    message.Topic,
                    kafkaMessage,
                    cancellationToken);

                // Проверяем статус доставки
                if (deliveryResult.Status == PersistenceStatus.Persisted)
                {
                    // Успех: отмечаем сообщение как обработанное
                    await outboxRepository.MarkAsProcessedAsync(
                        message.Id,
                        DateTimeOffset.UtcNow,
                        cancellationToken);

                    _logger.LogInformation("Сообщение {MessageId} успешно отправлено в Kafka", message.Id);
                }
                else
                {
                    // Сбой: ProduceAsync завершился, но статус не "Persisted"
                    // (например, из-за переполненной локальной очереди)
                    var errorReason = $"Сообщение не было сохранено в Kafka. Статус: {deliveryResult.Status}";
                    _logger.LogWarning("Сообщение {MessageId} не сохранено. Статус: {Status}", message.Id, deliveryResult.Status);

                    // Отмечаем сообщение как неудачное в Outbox
                    await outboxRepository.MarkAsFailedAsync(
                        message.Id,
                        errorReason,
                        cancellationToken);

                    var kafkaError = new Error(ErrorCode.Local_QueueFull, errorReason, false);
                    throw new KafkaException(kafkaError);
                }
            }
            // Ловим специфическое исключение от Kafka-продюсера
            catch (ProduceException<Null, string> ex)
            {
                // Этот блок ловит ошибки, которые бросает сама библиотека
                // (например, нет соединения с брокером, ошибка аутентификации и т.д.)
                _logger.LogError(ex, "Ошибка Kafka при отправке сообщения {MessageId}. Код: {ErrorCode}, Причина: {ErrorReason}",
                    message.Id, ex.Error.Code, ex.Error.Reason);

                // Отмечаем сообщение как неудачное в Outbox
                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    ex.Error.Reason,
                    cancellationToken);

                // Бросаем исключение дальше. Можно бросить `ex` или обернуть в KafkaException
                throw new KafkaException(ex.Error);
            }
            // Общий блок для любых других непредвиденных ошибок
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при отправке сообщения {MessageId} в Kafka", message.Id);

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    ex.Message,
                    cancellationToken);

                // Перебрасываем общее исключение
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _producer?.Flush(cancellationToken);
            _producer?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}