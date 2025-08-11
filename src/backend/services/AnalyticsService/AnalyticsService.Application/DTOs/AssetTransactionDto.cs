namespace StockMarketAssistant.AnalyticsService.Application.DTOs
{
    /// <summary>
    /// DTO для транзакции с активом
    /// </summary>
    public class AssetTransactionDto
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
        public string AssetType { get; set; } = string.Empty;

        /// <summary>
        /// Тип транзакции (покупка/продажа)
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;

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
}
