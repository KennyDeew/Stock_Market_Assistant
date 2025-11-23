using StockMarketAssistant.AnalyticsService.Domain.Events;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс для публикации и подписки на доменные события
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Публикация события
        /// </summary>
        /// <typeparam name="TEvent">Тип события</typeparam>
        /// <param name="event">Событие для публикации</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : Domain.Events.IDomainEvent;

        /// <summary>
        /// Подписка на событие
        /// </summary>
        /// <typeparam name="TEvent">Тип события</typeparam>
        /// <typeparam name="THandler">Тип обработчика</typeparam>
        void Subscribe<TEvent, THandler>()
            where TEvent : Domain.Events.IDomainEvent
            where THandler : IEventHandler<TEvent>;
    }
}

