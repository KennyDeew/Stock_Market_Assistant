using System;

namespace StockMarketAssistant.StockCardService.Models
{
    public class Dividend
    {
        public Guid Id { get; set; }
        public Guid ShareCardId { get; set; }
        public string Period { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
        
        public ShareCard ShareCard { get; set; }
    }
}
