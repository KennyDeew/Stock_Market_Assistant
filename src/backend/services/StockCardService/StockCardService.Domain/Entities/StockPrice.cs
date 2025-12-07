namespace StockMarketAssistant.StockCardService.WebApi.Models
{
    /// <summary>
    /// Сущность для обновления цены актива
    /// </summary>
    public class StockPrice
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
        /// Дата
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
