using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    public interface IAlertRepository : IRepository<Alert, Guid>
    {
        Task<IEnumerable<Alert>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Alert>> GetActiveByUserIdAsync(Guid userId);
        Task<IEnumerable<Alert>> GetPendingAlertsAsync();
        Task<bool> ExistsByStockCardIdAsync(Guid portfolioAssetId, Guid userId);
    }
}