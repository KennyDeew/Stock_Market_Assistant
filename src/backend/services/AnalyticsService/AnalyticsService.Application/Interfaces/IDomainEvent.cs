using StockMarketAssistant.AnalyticsService.Domain.Events;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для всех доменных событий
    /// Алиас для Domain.Events.IDomainEvent для обратной совместимости
    /// </summary>
    public interface IDomainEvent : Domain.Events.IDomainEvent
    {
    }
}

