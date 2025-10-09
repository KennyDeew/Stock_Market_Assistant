using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий EF Core для финансовых активов портфелей ценных бумаг
    /// </summary>
    /// <param name="dataContext"></param>
    public class PortfolioAssetRepository(DatabaseContext dataContext) : EfRepository<PortfolioAsset, Guid>(dataContext), IPortfolioAssetRepository
    {
    }
}
