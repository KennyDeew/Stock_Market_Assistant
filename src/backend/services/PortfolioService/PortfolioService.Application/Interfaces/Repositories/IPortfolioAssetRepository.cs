
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с финансовыми активами в портфеле ценных бумаг
    /// </summary>
    public interface IPortfolioAssetRepository : IRepository<PortfolioAsset, Guid>
    {
        /// <summary>
        /// Получить актив по идентификаторам портфеля и карточки ценной бумаги
        /// </summary>
        Task<PortfolioAsset?> GetByPortfolioAndStockCardAsync(Guid portfolioId, Guid stockCardId, bool includeTransactions = false);

        ///// <summary>
        ///// Существует ли транзакция актива портфеля
        ///// </summary>
        //Task<bool> ExistsTransactionAsync(Guid transactionId);

        /// <summary>
        /// Получить транзакцию по идентификатору
        /// </summary>
        Task<PortfolioAssetTransaction?> GetAssetTransactionByIdAsync(Guid transactionId);

        /// <summary>
        /// Получить все транзакции актива
        /// </summary>
        Task<IEnumerable<PortfolioAssetTransaction>> GetAssetTransactionsByAssetIdAsync(Guid assetId);

        /// <summary>
        /// Получить транзакции актива за период
        /// </summary>
        Task<IEnumerable<PortfolioAssetTransaction>> GetAssetTransactionsByAssetIdAndPeriodAsync(
            Guid assetId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Добавить транзакцию к активу
        /// </summary>
        Task<PortfolioAssetTransaction> AddAssetTransactionAsync(PortfolioAssetTransaction transaction);

        /// <summary>
        /// Добавить несколько транзакций к активу
        /// </summary>
        Task AddAssetTransactionsAsync(IEnumerable<PortfolioAssetTransaction> transactions);

        /// <summary>
        /// Редактировать транзакцию актива
        /// </summary>
        Task UpdateAssetTransactionAsync(PortfolioAssetTransaction transaction);

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        Task DeleteAssetTransactionAsync(Guid transactionId);

        /// <summary>
        /// Получить общее количество транзакций по активу
        /// </summary>
        Task<int> GetAssetTransactionsCountAsync(Guid assetId);
    }
}
