using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность транзакции для финансового актива из портфеля
    /// </summary>
    /// <remarks>
    /// Конструктор
    /// </remarks>
    /// <param name="id">Id транзакции</param>
    /// <param name="portfolioAssetId">Id актива портфеля</param>
    /// <param name="transactionType">Тип транзакции</param>
    /// <param name="quantity">Количество</param>
    /// <param name="pricePerUnit">Цена за единицу</param>
    /// <param name="currency">Валюта транзакции</param>
    /// <param name="transactionDate">Дата транзакции</param>
    public class PortfolioAssetTransaction(
        Guid id,
        Guid portfolioAssetId,
        PortfolioAssetTransactionType transactionType,
        int quantity,
        decimal pricePerUnit,
        DateTime transactionDate,
        string currency = "RUB") : BaseEntity<Guid>(id)
    {
        /// <summary>
        /// Id актива портфеля
        /// </summary>
        public Guid PortfolioAssetId { get; set; } = portfolioAssetId;

        /// <summary>
        /// Финансовый актив портфеля
        /// </summary>
        public virtual PortfolioAsset? PortfolioAsset { get; set; }

        /// <summary>
        /// Тип транзакции
        /// </summary>
        public PortfolioAssetTransactionType TransactionType { get; set; } = transactionType;

        /// <summary>
        /// Количество, шт.
        /// </summary>
        public int Quantity { get; set; } = quantity;

        /// <summary>
        /// Цена за единицу актива
        /// </summary>
        public decimal PricePerUnit { get; set; } = pricePerUnit;

        /// <summary>
        /// Валюта транзакции (RUB, USD и т.д.)
        /// </summary>
        public string Currency { get; set; } = currency;

        /// <summary>
        /// Дата транзакции
        /// </summary>
        public DateTime TransactionDate { get; set; } = transactionDate;

    }
}
