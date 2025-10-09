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
        public required string Ticker { get; set; }

        /// <summary>
        /// Наименование акции
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Описание акции
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public required string Currency { get; set; }

        /// <summary>
        /// Текущая цена
        /// </summary>
        public decimal CurrentPrice { get; set; }
    }
}
