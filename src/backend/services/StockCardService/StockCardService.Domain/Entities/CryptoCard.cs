using StockMarketAssistant.StockCardService.Domain.Interfaces;

namespace StockMarketAssistant.StockCardService.Domain.Entities
{
    public class CryptoCard : IEntity<Guid>
    {
        /// <summary>
        /// Id криптовалюты
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Тикер криптовалюты
        /// </summary>
        public required string Ticker { get; set; }

        /// <summary>
        /// Наименование криптовалюты
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Описание криптовалюты
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }
    }
}

