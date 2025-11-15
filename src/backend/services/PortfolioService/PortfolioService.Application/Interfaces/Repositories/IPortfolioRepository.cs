
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    /// <summary>
    /// Интерфейс репозитория для работы с портфелями ценных бумаг
    /// </summary>
    public interface IPortfolioRepository: IRepository<Portfolio, Guid>
    {
        /// <summary>
        /// Получить все портфели пользователя
        /// </summary>
        Task<IEnumerable<Portfolio>> GetByUserIdAsync(Guid userId);
    }
}
