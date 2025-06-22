namespace StockCardService.WebApi.Models.CryptoCard
{
    /// <summary>
    /// Модель создаваемой карточки криптовалюты
    /// </summary>
    public class CreatingCryptoCardModel
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
