namespace StockMarketAssistant.AnalyticsService.Domain.Entities
{
    /// <summary>
    /// Доменная сущность рейтинга актива
    /// </summary>
    public class AssetRating : BaseEntity<Guid>
    {
        /// <summary>
        /// Идентификатор актива
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип актива
        /// </summary>
        public AssetType AssetType { get; set; }

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
        /// Время последнего обновления
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Контекст анализа (портфель или глобальный)
        /// </summary>
        public AnalysisContext Context { get; set; }

        /// <summary>
        /// Идентификатор портфеля (если анализ в контексте портфеля)
        /// </summary>
        public Guid? PortfolioId { get; set; }
    }

    /// <summary>
    /// Контекст анализа
    /// </summary>
    public enum AnalysisContext
    {
        Portfolio = 1,  // Анализ в контексте портфеля
        Global = 2      // Глобальный анализ по всем портфелям
    }
}

