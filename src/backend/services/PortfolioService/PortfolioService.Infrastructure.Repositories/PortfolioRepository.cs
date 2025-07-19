using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий EF Core для портфелей ценных бумаг
    /// </summary>
    /// <param name="dataContext"></param>
    public class PortfolioRepository(DatabaseContext dataContext) : EfRepository<Portfolio, Guid>(dataContext), IPortfolioRepository
    {
    }
}
