using StockMarketAssistant.AnalyticsService.Domain.Entities;

namespace StockMarketAssistant.AnalyticsService.Domain.Events
{
    /// <summary>
    /// Доменное событие, возникающее при получении новой транзакции
    /// </summary>
    public class TransactionReceivedEvent : IDomainEvent
    {
        /// <summary>
        /// Идентификатор события
        /// </summary>
        public Guid EventId { get; }

        /// <summary>
        /// Время возникновения события
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Транзакция, которая была получена
        /// </summary>
        public AssetTransaction Transaction { get; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="transaction">Транзакция</param>
        public TransactionReceivedEvent(AssetTransaction transaction)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }
    }
}

