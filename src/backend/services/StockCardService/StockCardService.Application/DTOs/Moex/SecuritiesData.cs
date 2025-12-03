namespace StockMarketAssistant.StockCardService.Application.DTOs.Moex
{
    public class SecuritiesData
    {
        public required List<string> Columns { get; set; }
        public required List<List<object>> Data { get; set; }
    }
}
