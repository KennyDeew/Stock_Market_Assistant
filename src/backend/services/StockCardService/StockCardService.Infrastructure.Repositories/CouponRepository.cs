using Microsoft.EntityFrameworkCore;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.Repositories
{
    public class CouponRepository : Repository<Coupon, Guid>, ISubRepository<Coupon, Guid>
    {
        public CouponRepository(StockCardDbContext context) : base(context)
        {

        }

        public async Task<List<Coupon>> GetAllByParentIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking = false)
        {
            return await GetAll().Where(d => d.ParentId == id).ToListAsync(cancellationToken);
        }
    }
}
