using System;

namespace StockMarketAssistant.StockCardService.Models
{
    public class CryptoCard
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
