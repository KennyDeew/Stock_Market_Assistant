using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с рейтингами активов
    /// </summary>
    public interface IAssetRatingRepository
    {
        /// <summary>
        /// Получить рейтинг по идентификатору
        /// </summary>
        Task<AssetRating?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все рейтинги
        /// </summary>
        Task<IEnumerable<AssetRating>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить рейтинг по идентификатору актива, периоду и контексту
        /// </summary>
        Task<AssetRating?> GetByStockCardAndPeriodAsync(
            Guid stockCardId,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить рейтинги по идентификатору портфеля
        /// </summary>
        Task<IEnumerable<AssetRating>> GetByPortfolioIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить рейтинги по идентификатору портфеля за период
        /// </summary>
        Task<IEnumerable<AssetRating>> GetByPortfolioAndPeriodAsync(
            Guid portfolioId,
            Period period,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить рейтинги по контексту и периоду
        /// </summary>
        Task<IEnumerable<AssetRating>> GetByContextAndPeriodAsync(
            AnalysisContext context,
            Period period,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить топ активов по количеству покупок
        /// </summary>
        Task<IEnumerable<AssetRating>> GetTopBoughtAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить топ активов по количеству продаж
        /// </summary>
        Task<IEnumerable<AssetRating>> GetTopSoldAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить топ активов по сумме покупок
        /// </summary>
        Task<IEnumerable<AssetRating>> GetTopByBuyAmountAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить топ активов по сумме продаж
        /// </summary>
        Task<IEnumerable<AssetRating>> GetTopBySellAmountAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавить рейтинг
        /// </summary>
        Task AddAsync(AssetRating rating, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавить несколько рейтингов
        /// </summary>
        Task AddRangeAsync(IEnumerable<AssetRating> ratings, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить рейтинг
        /// </summary>
        Task UpdateAsync(AssetRating rating, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить или добавить рейтинг (Upsert)
        /// </summary>
        Task UpsertAsync(AssetRating rating, CancellationToken cancellationToken = default);

        /// <summary>
        /// Пакетное обновление или добавление рейтингов (оптимизировано через PostgreSQL ON CONFLICT)
        /// </summary>
        Task UpsertBatchAsync(IEnumerable<AssetRating> ratings, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить рейтинг
        /// </summary>
        Task DeleteAsync(AssetRating rating, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить рейтинг по идентификатору
        /// </summary>
        Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

