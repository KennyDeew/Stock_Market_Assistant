namespace StockMarketAssistant.AnalyticsService.Application.DTOs
{
    /// <summary>
    /// DTO для рейтинга активов
    /// </summary>
    public class AssetRatingDto
    {
        /// <summary>
        /// Идентификатор актива
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип актива
        /// </summary>
        public string AssetType { get; set; } = string.Empty;

        /// <summary>
        /// Тикер актива
        /// </summary>
        public string Ticker { get; set; } = string.Empty;

        /// <summary>
        /// Название актива
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Период анализа (начало)
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Период анализа (конец)
        /// </summary>
        public DateTime PeriodEnd { get; set; }

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

        /// <summary>
        /// Общее количество купленных активов
        /// </summary>
        public int TotalBuyQuantity { get; set; }

        /// <summary>
        /// Общее количество проданных активов
        /// </summary>
        public int TotalSellQuantity { get; set; }

        /// <summary>
        /// Рейтинг по количеству транзакций
        /// </summary>
        public int TransactionCountRank { get; set; }

        /// <summary>
        /// Рейтинг по стоимости транзакций
        /// </summary>
        public int TransactionAmountRank { get; set; }

        /// <summary>
        /// Контекст анализа
        /// </summary>
        public string Context { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор портфеля (если анализ в контексте портфеля)
        /// </summary>
        public Guid? PortfolioId { get; set; }
    }
}
