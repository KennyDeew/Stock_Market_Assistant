using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.Repositories
{
    public class BondCardRepository : Repository<BondCard, Guid>
    {
        public BondCardRepository(StockCardDbContext context) : base(context)
        {

        }
    }
}
