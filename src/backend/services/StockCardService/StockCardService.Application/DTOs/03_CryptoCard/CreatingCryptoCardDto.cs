namespace StockMarketAssistant.StockCardService.Application.DTOs._03_CryptoCard
{
    /// <summary>
    /// Dto создаваемой карточки криптовалюты
    /// </summary>
    public class CreatingCryptoCardDto
    {
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
