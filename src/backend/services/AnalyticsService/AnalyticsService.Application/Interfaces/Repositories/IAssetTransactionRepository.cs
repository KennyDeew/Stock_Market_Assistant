using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с транзакциями активов
    /// </summary>
    public interface IAssetTransactionRepository
    {
        /// <summary>
        /// Получить транзакцию по идентификатору
        /// </summary>
        Task<AssetTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все транзакции
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции по идентификатору портфеля
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByPortfolioIdAsync(Guid portfolioId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции по идентификатору актива
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByStockCardIdAsync(Guid stockCardId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции за период
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByPeriodAsync(Period period, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции по идентификатору портфеля за период
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByPortfolioAndPeriodAsync(
            Guid portfolioId,
            Period period,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции по идентификатору актива за период
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByStockCardAndPeriodAsync(
            Guid stockCardId,
            Period period,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции по типу транзакции
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByTransactionTypeAsync(
            TransactionType transactionType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции за период с фильтрацией по типу транзакции
        /// </summary>
        Task<IEnumerable<AssetTransaction>> GetByPeriodAndTypeAsync(
            Period period,
            TransactionType? transactionType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить транзакции, сгруппированные по StockCardId
        /// </summary>
        Task<IEnumerable<IGrouping<Guid, AssetTransaction>>> GetGroupedByStockCardAsync(
            Period? period = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавить транзакцию
        /// </summary>
        Task AddAsync(AssetTransaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавить несколько транзакций
        /// </summary>
        Task AddRangeAsync(IEnumerable<AssetTransaction> transactions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить транзакцию
        /// </summary>
        Task UpdateAsync(AssetTransaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        Task DeleteAsync(AssetTransaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить транзакцию по идентификатору
        /// </summary>
        Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

