namespace StockCardService.Domain.Entities
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
