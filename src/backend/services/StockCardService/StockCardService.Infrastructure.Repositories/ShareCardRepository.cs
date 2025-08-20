using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий работы с акциями
    /// </summary>
    public class ShareCardRepository : Repository<ShareCard, Guid>
    {
        public ShareCardRepository(StockCardDbContext context) : base(context)
        {

        }
    }
}
