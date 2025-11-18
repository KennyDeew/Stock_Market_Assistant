namespace StockMarketAssistant.StockCardService.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс сущности с идентификатором.
    /// </summary>
    /// <typeparam name="TId"> Тип идентификатора. </typeparam>
    public interface IEntityWithParent<TId>
    {
        /// <summary>
        /// Идентификатор.
        /// </summary>
        TId ParentId { get; set; }
    }
}
