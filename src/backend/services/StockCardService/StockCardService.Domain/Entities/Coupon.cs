using StockCardService.Domain.Entities;
using System;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class Coupon : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid BondId { get; set; }
        public DateTime Period { get; set; }

        public string Currency { get; set; }
        public decimal Value { get; set; }
        
        public BondCard Bond { get; set; }
    }
}

