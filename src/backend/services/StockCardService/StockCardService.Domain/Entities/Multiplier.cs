using System;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class Multiplier
    {
        public Guid Id { get; set; }
        public Guid ShareCardId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        
        public ShareCard ShareCard { get; set; }
    }
}

