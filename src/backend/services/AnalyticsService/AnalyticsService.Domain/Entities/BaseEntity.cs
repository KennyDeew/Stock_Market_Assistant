namespace StockMarketAssistant.AnalyticsService.Domain.Entities
{
    /// <summary>
    /// Базовая сущность для всех доменных объектов
    /// </summary>
    /// <typeparam name="TId">Тип идентификатора</typeparam>
    public abstract class BaseEntity<TId>
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public TId Id { get; protected set; } = default!;

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public DateTime UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        protected BaseEntity(TId id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// Обновляет время последнего изменения
        /// </summary>
        protected void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
