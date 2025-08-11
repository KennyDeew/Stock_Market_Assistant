using StockMarketAssistant.AnalyticsService.Application.DTOs;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса рейтинга активов
    /// </summary>
    public interface IAssetRatingService
    {
        /// <summary>
        /// Получает рейтинг активов по заданным критериям
        /// </summary>
        /// <param name="request">Параметры запроса</param>
        /// <returns>Список рейтингов активов</returns>
        Task<IEnumerable<AssetRatingDto>> GetAssetRatingsAsync(AssetRatingRequestDto request);

        /// <summary>
        /// Получает топ самых покупаемых активов за период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="limit">Количество записей</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список самых покупаемых активов</returns>
        Task<IEnumerable<AssetRatingDto>> GetTopBuyingAssetsAsync(DateTime periodStart, DateTime periodEnd, int limit = 10, Guid? portfolioId = null);

        /// <summary>
        /// Получает топ самых продаваемых активов за период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="limit">Количество записей</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список самых продаваемых активов</returns>
        Task<IEnumerable<AssetRatingDto>> GetTopSellingAssetsAsync(DateTime periodStart, DateTime periodEnd, int limit = 10, Guid? portfolioId = null);

        /// <summary>
        /// Обновляет рейтинги активов на основе новых транзакций
        /// </summary>
        /// <param name="transaction">Новая транзакция</param>
        /// <returns>Задача обновления</returns>
        Task UpdateAssetRatingsAsync(AssetTransactionDto transaction);

        /// <summary>
        /// Пересчитывает рейтинги за указанный период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Задача пересчета</returns>
        Task RecalculateRatingsAsync(DateTime periodStart, DateTime periodEnd, Guid? portfolioId = null);
    }
}
