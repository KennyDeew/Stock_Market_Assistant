using Microsoft.EntityFrameworkCore;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.Repositories
{
    public class DividendRepository : Repository<Dividend, Guid>, ISubRepository<Dividend, Guid>
    {
        public DividendRepository(StockCardDbContext context) : base(context)
        {

        }

        public async Task<List<Dividend>> GetAllByParentIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
        {
            return await GetAll().Where(d => d.ParentId == id).ToListAsync(cancellationToken);
        }
    }
}
