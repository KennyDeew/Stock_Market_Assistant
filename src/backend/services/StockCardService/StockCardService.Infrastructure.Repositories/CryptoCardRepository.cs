using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.Repositories
{
    public class CryptoCardRepository : Repository<CryptoCard, Guid>
    {
        public CryptoCardRepository(StockCardDbContext context) : base(context)
        {

        }
    }
}
