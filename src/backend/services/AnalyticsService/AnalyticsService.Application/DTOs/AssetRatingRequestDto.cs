namespace StockMarketAssistant.AnalyticsService.Application.DTOs
{
    /// <summary>
    /// DTO для запроса рейтинга активов
    /// </summary>
    public class AssetRatingRequestDto
    {
        /// <summary>
        /// Дата начала периода анализа
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Дата окончания периода анализа
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Тип актива для фильтрации (опционально)
        /// </summary>
        public string? AssetType { get; set; }

        /// <summary>
        /// Идентификатор портфеля для анализа (опционально, если не указан - глобальный анализ)
        /// </summary>
        public Guid? PortfolioId { get; set; }

        /// <summary>
        /// Количество записей для возврата (по умолчанию 100)
        /// </summary>
        public int Limit { get; set; } = 100;

        /// <summary>
        /// Смещение для пагинации (по умолчанию 0)
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Сортировка по рейтингу (по умолчанию по количеству транзакций)
        /// </summary>
        public RatingSortType SortBy { get; set; } = RatingSortType.TransactionCount;

        /// <summary>
        /// Направление сортировки (по умолчанию по убыванию)
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    }

    /// <summary>
    /// Тип сортировки рейтинга
    /// </summary>
    public enum RatingSortType
    {
        TransactionCount = 1,    // По количеству транзакций
        TransactionAmount = 2,   // По стоимости транзакций
        BuyCount = 3,            // По количеству покупок
        SellCount = 4,           // По количеству продаж
        BuyAmount = 5,           // По стоимости покупок
        SellAmount = 6           // По стоимости продаж
    }

    /// <summary>
    /// Направление сортировки
    /// </summary>
    public enum SortDirection
    {
        Ascending = 1,   // По возрастанию
        Descending = 2   // По убыванию
    }
}
