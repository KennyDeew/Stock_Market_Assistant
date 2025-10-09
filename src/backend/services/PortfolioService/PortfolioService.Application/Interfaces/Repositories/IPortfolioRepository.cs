
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с портфелями ценных бумаг
    /// </summary>
    public interface IPortfolioRepository: IRepository<Portfolio, Guid>
    {
    }
}
