using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий EF Core для работы с исходящими сообщениями (Outbox)
    /// </summary>    
    public class OutboxRepository(DatabaseContext context) : EfRepository<OutboxMessage, Guid>(context), IOutboxRepository
    {
        /// <summary>
        /// Получить необработанные сообщения
        /// </summary>
        /// <param name="batchSize">Размер пачки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Коллекция необработанных сообщений</returns>
        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(
            int batchSize = 100,
            CancellationToken cancellationToken = default)
        {
            return await Data
                .Where(x => x.ProcessedAt == null && x.RetryCount < 3)
                .OrderBy(x => x.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Пометить сообщение как обработанное
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <param name="processedAt">Время обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task MarkAsProcessedAsync(Guid messageId, DateTimeOffset processedAt, CancellationToken cancellationToken = default)
        {
            var message = await Data
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (message != null)
            {
                message.MarkAsProcessed();
                await UpdateAsync(message);
            }
        }

        /// <summary>
        /// Пометить сообщение как неудачное
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <param name="error">Текст ошибки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
        {
            var message = await Data
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (message != null)
            {
                message.MarkAsFailed(error);
                await UpdateAsync(message);
            }
        }
    }
}
