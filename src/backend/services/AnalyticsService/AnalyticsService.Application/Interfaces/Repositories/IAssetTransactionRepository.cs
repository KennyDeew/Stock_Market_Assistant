using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для транзакций с активами
    /// </summary>
    public interface IAssetTransactionRepository
    {
        /// <summary>
        /// Получает транзакцию по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор транзакции</param>
        /// <returns>Транзакция</returns>
        Task<AssetTransaction?> GetByIdAsync(Guid id);

        /// <summary>
        /// Получает транзакции по параметрам
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <param name="stockCardId">Идентификатор актива (опционально)</param>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="assetType">Тип актива (опционально)</param>
        /// <param name="transactionType">Тип транзакции (опционально)</param>
        /// <returns>Список транзакций</returns>
        Task<IEnumerable<AssetTransaction>> GetByParametersAsync(
            Guid? portfolioId = null,
            Guid? stockCardId = null,
            DateTime? periodStart = null,
            DateTime? periodEnd = null,
            AssetType? assetType = null,
            TransactionType? transactionType = null);

        /// <summary>
        /// Получает агрегированные данные по транзакциям
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="assetType">Тип актива (опционально)</param>
        /// <returns>Агрегированные данные</returns>
        Task<TransactionAggregation> GetAggregatedDataAsync(
            Guid? portfolioId,
            DateTime periodStart,
            DateTime periodEnd,
            AssetType? assetType = null);

        /// <summary>
        /// Добавляет новую транзакцию
        /// </summary>
        /// <param name="transaction">Транзакция для добавления</param>
        /// <returns>Задача добавления</returns>
        Task AddAsync(AssetTransaction transaction);

        /// <summary>
        /// Обновляет существующую транзакцию
        /// </summary>
        /// <param name="transaction">Транзакция для обновления</param>
        /// <returns>Задача обновления</returns>
        Task UpdateAsync(AssetTransaction transaction);

        /// <summary>
        /// Удаляет транзакцию
        /// </summary>
        /// <param name="id">Идентификатор транзакции</param>
        /// <returns>Задача удаления</returns>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Сохраняет изменения в базе данных
        /// </summary>
        /// <returns>Задача сохранения</returns>
        Task SaveChangesAsync();
    }

    /// <summary>
    /// Агрегированные данные по транзакциям
    /// </summary>
    public class TransactionAggregation
    {
        /// <summary>
        /// Общее количество транзакций
        /// </summary>
        public int TotalTransactionCount { get; set; }

        /// <summary>
        /// Общая стоимость транзакций
        /// </summary>
        public decimal TotalTransactionAmount { get; set; }

        /// <summary>
        /// Количество транзакций покупки
        /// </summary>
        public int BuyTransactionCount { get; set; }

        /// <summary>
        /// Количество транзакций продажи
        /// </summary>
        public int SellTransactionCount { get; set; }

        /// <summary>
        /// Общая стоимость покупок
        /// </summary>
        public decimal TotalBuyAmount { get; set; }

        /// <summary>
        /// Общая стоимость продаж
        /// </summary>
        public decimal TotalSellAmount { get; set; }
    }
}
