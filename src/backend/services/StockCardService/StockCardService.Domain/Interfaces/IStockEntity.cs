namespace StockMarketAssistant.StockCardService.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс сущности с типом торгов
    /// </summary>
    public interface IStockEntity
    {
        /// <summary>
        /// Режим торгов (АКЦИИ - TQBR, КорпОбл - TQCB, ОФЗ - TQOB)
        /// </summary>
        string Board {  get; }
    }
}
