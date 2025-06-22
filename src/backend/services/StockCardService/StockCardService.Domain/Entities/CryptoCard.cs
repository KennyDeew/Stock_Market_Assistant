using StockCardService.Domain.Entities;
using System;

namespace StockMarketAssistant.StockCardService.Domain.Entities
{
    public class CryptoCard : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Ticker { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}

