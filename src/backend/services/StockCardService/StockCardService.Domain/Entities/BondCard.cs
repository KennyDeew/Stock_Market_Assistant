using System;
using System.Collections.Generic;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class BondCard
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MaturityPeriod { get; set; }
        
        public ICollection<Coupon> Coupons { get; set; }
    }
}

