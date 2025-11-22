namespace StockMarketAssistant.AnalyticsService.Domain.Events
{
    /// <summary>
    /// Базовый интерфейс для всех доменных событий
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Идентификатор события
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Время возникновения события
        /// </summary>
        DateTime OccurredOn { get; }
    }
}

