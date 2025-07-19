
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Репозиторий работы с портфелями ценных бумаг
    /// </summary>
    public interface IPortfolioRepository: IRepository<Portfolio, Guid>
    {
    }
}
