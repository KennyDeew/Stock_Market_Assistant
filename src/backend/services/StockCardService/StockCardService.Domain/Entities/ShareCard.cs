using System;
using System.Collections.Generic;

namespace StockMarketAssistant.StockCardService.Models
{
    public class ShareCard
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
