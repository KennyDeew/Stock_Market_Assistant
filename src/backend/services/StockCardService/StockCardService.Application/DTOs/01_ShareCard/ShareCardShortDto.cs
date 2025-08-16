namespace StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard
{
    /// <summary>
    /// Модель карточки акции
    /// </summary>
    public class ShareCardShortDto
    {
        /// <summary>
        /// Id акции
        /// </summary>
        public Guid Id { get; set; }

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

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; }
    }
}
