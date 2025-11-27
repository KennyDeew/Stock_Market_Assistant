using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с исходящими сообщениями (Outbox)
    /// </summary>    
    public interface IOutboxRepository : IRepository<OutboxMessage, Guid>
    {
        /// <summary>
        /// Получить необработанные сообщения
        /// </summary>
        /// <param name="batchSize">Размер пачки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция необработанных сообщений</returns>
        Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 100, CancellationToken cancellationToken = default);
        /// <summary>
        /// Пометить сообщение как обработанное
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <param name="processedAt">Время обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task MarkAsProcessedAsync(Guid messageId, DateTimeOffset processedAt, CancellationToken cancellationToken = default);
        /// <summary>
        /// Пометить сообщение как неудачное
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <param name="error">Текст ошибки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
    }
}
