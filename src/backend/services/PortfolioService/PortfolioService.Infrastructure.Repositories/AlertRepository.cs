using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий EF Core для оповещений
    /// </summary>
    public class AlertRepository(DatabaseContext context) : EfRepository<Alert, Guid>(context), IAlertRepository
    {
        public async Task<IEnumerable<Alert>> GetByUserIdAsync(Guid userId)
        {
            return await Data.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetPendingAlertsAsync()
        {
            return await Data
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Alert>> GetActiveByUserIdAsync(Guid userId)
        {
            return await Data
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();
        }

        public async Task<bool> ExistsByStockCardIdAsync(Guid stockCardId, Guid userId)
        {
            return await Data.AnyAsync(a => a.StockCardId == stockCardId && a.UserId == userId);
        }
    }
}
