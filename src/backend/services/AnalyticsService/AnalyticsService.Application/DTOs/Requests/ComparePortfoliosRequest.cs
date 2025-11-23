namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Requests
{
    /// <summary>
    /// DTO запроса для сравнения портфелей
    /// </summary>
    public class ComparePortfoliosRequest
    {
        /// <summary>
        /// Список идентификаторов портфелей для сравнения
        /// </summary>
        public List<Guid> PortfolioIds { get; set; } = new();

        /// <summary>
        /// Начальная дата периода (опционально)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Конечная дата периода (опционально)
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}

