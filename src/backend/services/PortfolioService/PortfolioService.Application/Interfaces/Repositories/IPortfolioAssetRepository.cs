
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий работы с финансовыми активами в портфеле ценных бумаг
    /// </summary>
    public interface IPortfolioAssetRepository : IRepository<PortfolioAsset, Guid>
    {
    }
}
