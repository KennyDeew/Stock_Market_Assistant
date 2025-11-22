namespace StockMarketAssistant.AnalyticsService.Domain.Enums
{
    /// <summary>
    /// Контекст анализа для определения области агрегации рейтингов
    /// </summary>
    public enum AnalysisContext
    {
        /// <summary>
        /// Глобальная агрегация по всем портфелям
        /// </summary>
        Global = 1,

        /// <summary>
        /// Агрегация для конкретного портфеля
        /// </summary>
        Portfolio = 2
    }
}

