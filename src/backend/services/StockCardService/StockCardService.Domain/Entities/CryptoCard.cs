using StockCardService.Domain.Entities;

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
        public string Ticker { get; set; }

        /// <summary>
        /// Наименование криптовалюты
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание криптовалюты
        /// </summary>
        public string Description { get; set; }
    }
}

