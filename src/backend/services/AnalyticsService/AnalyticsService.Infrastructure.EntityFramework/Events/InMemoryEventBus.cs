using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Domain.Events;
using System.Collections.Concurrent;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Events
{
    /// <summary>
    /// In-memory реализация EventBus для доменных событий
    /// </summary>
    public class InMemoryEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InMemoryEventBus> _logger;
        private readonly ConcurrentDictionary<Type, List<Type>> _handlers = new();

        public InMemoryEventBus(
            IServiceProvider serviceProvider,
            ILogger<InMemoryEventBus> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Публикация события
        /// </summary>
        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : Domain.Events.IDomainEvent
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            var eventType = typeof(TEvent);
            _logger.LogInformation("Публикация события: {EventType}, EventId: {EventId}", eventType.Name, @event.EventId);

            if (!_handlers.TryGetValue(eventType, out var handlerTypes))
            {
                _logger.LogWarning("Не найдено обработчиков для события: {EventType}", eventType.Name);
                return;
            }

            // Выполняем обработчики параллельно
            var tasks = handlerTypes.Select(async handlerType =>
            {
                try
                {
                    var handler = _serviceProvider.GetService(handlerType);
                    if (handler == null)
                    {
                        _logger.LogWarning("Не удалось получить обработчик {HandlerType} из DI", handlerType.Name);
                        return;
                    }

                    var handleMethod = handlerType.GetMethod("HandleAsync");
                    if (handleMethod == null)
                    {
                        _logger.LogWarning("Метод HandleAsync не найден в обработчике {HandlerType}", handlerType.Name);
                        return;
                    }

                    var task = (Task?)handleMethod.Invoke(handler, new object[] { @event, cancellationToken });
                    if (task != null)
                    {
                        await task;
                    }

                    _logger.LogInformation("Событие {EventType} успешно обработано обработчиком {HandlerType}",
                        eventType.Name, handlerType.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке события {EventType} обработчиком {HandlerType}",
                        eventType.Name, handlerType.Name);
                    // Не пробрасываем исключение, чтобы не прерывать обработку других обработчиков
                }
            });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Подписка на событие
        /// </summary>
        public void Subscribe<TEvent, THandler>()
            where TEvent : Domain.Events.IDomainEvent
            where THandler : IEventHandler<TEvent>
        {
            var eventType = typeof(TEvent);
            var handlerType = typeof(THandler);

            _handlers.AddOrUpdate(
                eventType,
                new List<Type> { handlerType },
                (key, existingHandlers) =>
                {
                    if (!existingHandlers.Contains(handlerType))
                    {
                        existingHandlers.Add(handlerType);
                    }
                    return existingHandlers;
                });

            _logger.LogInformation("Подписка на событие {EventType} обработчиком {HandlerType}",
                eventType.Name, handlerType.Name);
        }
    }
}

