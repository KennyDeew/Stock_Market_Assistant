namespace StockMarketAssistant.PortfolioService.Domain.Exceptions
{
    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
        public SecurityException(string message, Exception inner) : base(message, inner) { }
    }
}
