namespace StockMarketAssistant.StockCardService.Application.DTOs._03_CryptoCard
{
    /// <summary>
    /// Dto изменяемой карточки криптовалюты
    /// </summary>
    public class UpdatingCryptoCard
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
