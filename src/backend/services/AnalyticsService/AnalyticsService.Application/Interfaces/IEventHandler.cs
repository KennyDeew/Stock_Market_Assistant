using StockMarketAssistant.AnalyticsService.Domain.Events;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс обработчика доменных событий
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    public interface IEventHandler<in TEvent>
        where TEvent : Domain.Events.IDomainEvent
    {
        /// <summary>
        /// Обработка события
        /// </summary>
        /// <param name="event">Событие для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}

