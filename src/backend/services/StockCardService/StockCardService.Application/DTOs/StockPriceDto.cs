namespace StockMarketAssistant.StockCardService.Application.DTOs
{
    /// <summary>
    /// Dto сущности для обновления цены актива
    /// </summary>
    public class StockPriceDto
    {
        /// <summary>
        /// Тикер актива
        /// </summary>
        public required string Ticker { get; set; }
        /// <summary>
        /// Цена актива
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Изменение цены актива
        /// </summary>
        public decimal ChangePrice { get; set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
