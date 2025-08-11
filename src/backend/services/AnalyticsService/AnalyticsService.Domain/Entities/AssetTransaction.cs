namespace StockMarketAssistant.AnalyticsService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность транзакции с активом
    /// </summary>
    public class AssetTransaction : BaseEntity<Guid>
    {
        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        public Guid PortfolioId { get; set; }

        /// <summary>
        /// Идентификатор актива
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип актива
        /// </summary>
        public AssetType AssetType { get; set; }

        /// <summary>
        /// Тип транзакции (покупка/продажа)
        /// </summary>
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Количество активов
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Цена за единицу актива
        /// </summary>
        public decimal PricePerUnit { get; set; }

        /// <summary>
        /// Общая стоимость транзакции
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Время транзакции
        /// </summary>
        public DateTime TransactionTime { get; set; }

        /// <summary>
        /// Валюта транзакции
        /// </summary>
        public string Currency { get; set; } = "RUB";

        /// <summary>
        /// Дополнительные метаданные
        /// </summary>
        public string? Metadata { get; set; }
    }

    /// <summary>
    /// Тип актива
    /// </summary>
    public enum AssetType
    {
        Share = 1,      // Акция
        Bond = 2,       // Облигация
        Crypto = 3      // Криптовалюта
    }

    /// <summary>
    /// Тип транзакции
    /// </summary>
    public enum TransactionType
    {
        Buy = 1,        // Покупка
        Sell = 2        // Продажа
    }
}
