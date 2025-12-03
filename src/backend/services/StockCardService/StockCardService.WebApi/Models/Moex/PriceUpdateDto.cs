namespace StockMarketAssistant.StockCardService.WebApi.Models.Moex
{
    public class PriceUpdateDto
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public string Time { get; set; }
        public long Volume { get; set; }
        public int NumTrades { get; set; }
    }
}
