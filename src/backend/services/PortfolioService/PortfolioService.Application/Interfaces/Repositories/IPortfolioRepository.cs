
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

        /// <summary>
        /// Получить портфель пользователя с активами
        /// </summary>
        Task<Portfolio?> GetByIdWithAssetsAsync(Guid id);

        /// <summary>
        /// Получить портфель пользователя с активами и транзакциями
        /// </summary>
        Task<Portfolio?> GetByIdWithAssetsAndTransactionsAsync(Guid id);

    }
}
