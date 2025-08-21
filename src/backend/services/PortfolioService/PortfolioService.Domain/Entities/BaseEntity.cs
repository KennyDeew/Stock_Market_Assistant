namespace StockMarketAssistant.PortfolioService.Domain.Entities
{
    /// <summary>
    /// Базовый класс доменной сущности
    /// </summary>
    /// <typeparam name="T">Тип идентификатора</typeparam>
    public class BaseEntity<T> where T : notnull
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public T Id { get; protected set; }

        protected BaseEntity(T id) => Id = id;
    }
}
