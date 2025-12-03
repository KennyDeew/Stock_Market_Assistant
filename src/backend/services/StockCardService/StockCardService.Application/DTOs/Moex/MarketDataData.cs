namespace StockMarketAssistant.StockCardService.Application.DTOs.Moex
{
    public class MarketDataData
    {
        public List<string> Columns { get; set; } = [];
        public List<List<object?>> Data { get; set; } = [];
    }
}
