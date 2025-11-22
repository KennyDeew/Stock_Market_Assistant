namespace StockMarketAssistant.AnalyticsService.Application.DTOs
{
    /// <summary>
    /// DTO для истории транзакций портфеля
    /// </summary>
    public class PortfolioHistoryDto
    {
        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        public Guid PortfolioId { get; set; }

        /// <summary>
        /// Начальная дата периода
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Конечная дата периода
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Список транзакций за период
        /// </summary>
        public List<PortfolioTransactionDto> Transactions { get; set; } = new();
    }

    /// <summary>
    /// DTO для транзакции портфеля
    /// </summary>
    public class PortfolioTransactionDto
    {
        /// <summary>
        /// Идентификатор транзакции
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор актива портфеля
        /// </summary>
        public Guid PortfolioAssetId { get; set; }

        /// <summary>
        /// Идентификатор актива (StockCardId)
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип транзакции (1 = Buy, 2 = Sell)
        /// </summary>
        public int TransactionType { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Цена за единицу
        /// </summary>
        public decimal PricePerUnit { get; set; }

        /// <summary>
        /// Общая стоимость транзакции
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Дата и время транзакции
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; } = "RUB";
    }
}

