using System;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class Coupon
    {
        public Guid Id { get; set; }
        public Guid BondId { get; set; }
        public string Period { get; set; }
        public string Currency { get; set; }
        public decimal Value { get; set; }
        
        public BondCard Bond { get; set; }
    }
}

