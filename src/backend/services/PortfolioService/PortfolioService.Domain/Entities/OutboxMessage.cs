namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Сообщение для исходящей очереди (Outbox)
    /// </summary>
    /// <remarks>
    /// Конструктор сообщения Outbox
    /// </remarks>
    /// <param name="id">Идентификатор сообщения</param>
    /// <param name="topic">Топик Kafka</param>
    /// <param name="message">Сериализованное сообщение</param>
    public class OutboxMessage(
        Guid id,
        string topic,
        string message) : BaseEntity<Guid>(id)
    {
        /// <summary>
        /// Топик Kafka
        /// </summary>
        public string Topic { get; private set; } = topic;

        /// <summary>
        /// Сериализованное сообщение
        /// </summary>
        public string Message { get; private set; } = message;

        /// <summary>
        /// Дата создания сообщения
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Дата обработки сообщения
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; private set; }

        /// <summary>
        /// Ошибка обработки (если есть)
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Количество попыток отправки
        /// </summary>
        public int RetryCount { get; private set; } = 0;

        /// <summary>
        /// Пометить сообщение как обработанное
        /// </summary>
        public void MarkAsProcessed()
        {
            ProcessedAt = DateTimeOffset.UtcNow;
            Error = null;
        }

        /// <summary>
        /// Пометить сообщение как неудачное
        /// </summary>
        /// <param name="error">Текст ошибки</param>
        /// <param name="maxRetries">Максимальное количество попыток</param>
        public void MarkAsFailed(string error, int maxRetries = 3)
        {
            RetryCount++;
            Error = error;

            if (RetryCount >= maxRetries)
            {
                ProcessedAt = DateTimeOffset.UtcNow; // Помечаем как обработанное после исчерпания попыток
            }
        }
    }
}