namespace StockMarketAssistant.StockCardService.Application.DTOs.Moex
{
    public class MoexTickerResponse
    {
        public MarketDataData MarketData { get; set; } = new();
    }
}
