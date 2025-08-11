using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для рейтинга активов
    /// </summary>
    public interface IAssetRatingRepository
    {
        /// <summary>
        /// Получает рейтинг актива по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор рейтинга</param>
        /// <returns>Рейтинг актива</returns>
        Task<AssetRating?> GetByIdAsync(Guid id);

        /// <summary>
        /// Получает рейтинги активов по параметрам
        /// </summary>
        /// <param name="stockCardId">Идентификатор актива</param>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="context">Контекст анализа</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список рейтингов</returns>
        Task<IEnumerable<AssetRating>> GetByParametersAsync(Guid stockCardId, DateTime periodStart, DateTime periodEnd, AnalysisContext context, Guid? portfolioId = null);

        /// <summary>
        /// Получает топ активов по количеству транзакций
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="limit">Количество записей</param>
        /// <param name="context">Контекст анализа</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список рейтингов</returns>
        Task<IEnumerable<AssetRating>> GetTopByTransactionCountAsync(DateTime periodStart, DateTime periodEnd, int limit, AnalysisContext context, Guid? portfolioId = null);

        /// <summary>
        /// Получает топ активов по стоимости транзакций
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="limit">Количество записей</param>
        /// <param name="context">Контекст анализа</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список рейтингов</returns>
        Task<IEnumerable<AssetRating>> GetTopByTransactionAmountAsync(DateTime periodStart, DateTime periodEnd, int limit, AnalysisContext context, Guid? portfolioId = null);

        /// <summary>
        /// Добавляет новый рейтинг актива
        /// </summary>
        /// <param name="assetRating">Рейтинг для добавления</param>
        /// <returns>Задача добавления</returns>
        Task AddAsync(AssetRating assetRating);

        /// <summary>
        /// Обновляет существующий рейтинг актива
        /// </summary>
        /// <param name="assetRating">Рейтинг для обновления</param>
        /// <returns>Задача обновления</returns>
        Task UpdateAsync(AssetRating assetRating);

        /// <summary>
        /// Удаляет рейтинг актива
        /// </summary>
        /// <param name="id">Идентификатор рейтинга</param>
        /// <returns>Задача удаления</returns>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Сохраняет изменения в базе данных
        /// </summary>
        /// <returns>Задача сохранения</returns>
        Task SaveChangesAsync();
    }
}
