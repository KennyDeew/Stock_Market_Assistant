using StockCardService.Domain.Entities;
using System;
using System.Collections.Generic;

namespace StockMarketAssistant.StockCardService.Domain.Entities

{
    public class ShareCard : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public ICollection<FinancialReport> FinancialReports { get; set; }
        public ICollection<Multiplier> Multipliers { get; set; }
        public ICollection<Dividend> Dividends { get; set; }
    }
}

