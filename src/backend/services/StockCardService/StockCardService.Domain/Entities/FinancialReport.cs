using System;

namespace StockMarketAssistant.StockCardService.Domain.Entities
{
    public class FinancialReport
    {
        public Guid Id { get; set; }
        public Guid ShareCardId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Period { get; set; }
        public decimal Revenue { get; set; }
        public decimal EBITDA { get; set; }
        public decimal NetProfit { get; set; }
        public decimal CAPEX { get; set; }
        public decimal FCF { get; set; }
        public decimal Debt { get; set; }
        
        public ShareCard ShareCard { get; set; }
    }
}

