using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Requests
{
    /// <summary>
    /// DTO запроса для получения топ активов
    /// </summary>
    public class GetTopAssetsRequest
    {
        /// <summary>
        /// Начальная дата периода
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Конечная дата периода
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Количество топ активов (по умолчанию 10)
        /// </summary>
        public int Top { get; set; } = Domain.Constants.DomainConstants.Aggregation.DefaultTopAssetsCount;

        /// <summary>
        /// Контекст анализа (Global или Portfolio)
        /// </summary>
        public AnalysisContext Context { get; set; } = AnalysisContext.Global;

        /// <summary>
        /// Идентификатор портфеля (обязателен для Portfolio контекста)
        /// </summary>
        public Guid? PortfolioId { get; set; }
    }
}

