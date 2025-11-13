using StockMarketAssistant.StockCardService.Domain.Interfaces;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class ShareCard : IEntity<Guid>, IStockEntity
    {
        /// <summary>
        /// Id акции
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер акции
        /// </summary>
        public required string Ticker { get; set; }

        /// <summary>
        /// Наименование акции
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Режим торгов (АКЦИИ - TQBR, КорпОбл - TQCB, ОФЗ - TQOB)
        /// </summary>
        public string Board => "TQBR";

        /// <summary>
        /// Описание акции
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Валюта акции
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }

        /// <summary>
        /// Массив мультипликаторов
        /// </summary>
        public ICollection<Multiplier> Multipliers { get; set; } = new List<Multiplier>();

        /// <summary>
        /// Массив дивидендов
        /// </summary>
        public ICollection<Dividend> Dividends { get; set; } = new List<Dividend>();
    }
}

