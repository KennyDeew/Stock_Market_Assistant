namespace StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard
{
    /// <summary>
    /// Dto создаваемой карточки акции
    /// </summary>
    public class CreatingShareCardDto
    {
        /// <summary>
        /// Тикер акции
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Наименование акции
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание акции
        /// </summary>
        public string Description { get; set; }
    }
}
