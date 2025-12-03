namespace StockMarketAssistant.AnalyticsService.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при нарушении правил безопасности
    /// </summary>
    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception inner) : base(message, inner) { }
    }
}

