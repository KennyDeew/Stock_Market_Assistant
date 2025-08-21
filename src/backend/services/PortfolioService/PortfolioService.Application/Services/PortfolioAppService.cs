using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    /// <summary>
    /// Сервис работы с портфелями ценных бумаг
    /// </summary>
    public class PortfolioAppService(IPortfolioRepository portfolioRepository, IPortfolioAssetAppService portfolioAssetAppService) : IPortfolioAppService
    {
        private readonly IPortfolioRepository _portfolioRepository = portfolioRepository;
        private readonly IPortfolioAssetAppService _portfolioAssetAppService = portfolioAssetAppService;

        /// <summary>
        /// Создать портфель ценных бумаг
        /// </summary>
        /// <param name="creatingPortfolioDto">DTO создаваемого портфеля</param>
        /// <returns>Идентификатор созданного портфеля</returns>
        public async Task<Guid> CreateAsync(CreatingPortfolioDto creatingPortfolioDto)
        {
            Portfolio portfolio = new(Guid.NewGuid(), creatingPortfolioDto.UserId)
            {
                Name = creatingPortfolioDto.Name,
                Currency = creatingPortfolioDto.Currency
            };
            Portfolio createdPortfolio = await _portfolioRepository.AddAsync(portfolio);
            return createdPortfolio.Id;
        }

        /// <summary>
        /// Удалить портфель ценных бумаг
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(id);
            if (portfolio is not null)
            {
                await _portfolioRepository.DeleteAsync(portfolio);
            }
        }

        /// <summary>
        /// Получить перечень всех портфелей ценных бумаг для всех пользователей
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PortfolioDto>> GetAllAsync()
        {
            IEnumerable<Portfolio> portfolios = await _portfolioRepository.GetAllAsync();
            return portfolios.Select(p =>
                new PortfolioDto() {
                    Id = p.Id, UserId = p.UserId,
                    Name = p.Name, Currency = p.Currency
                });
        }

        /// <summary>
        /// Получить портфель ценных бумаг
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns></returns>
        public async Task<PortfolioDto?> GetByIdAsync(Guid id)
        {
            Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(id, p => p.Assets);
            if (portfolio is null)
                return null;
            // Получаем все асинхронные задачи для активов
            var assetTasks = portfolio.Assets.Select(a => _portfolioAssetAppService.GetByIdAsync(a.Id));
            // Дожидаемся завершения всех задач
            var assets = await Task.WhenAll(assetTasks);

            return new PortfolioDto()
            {
                Id = portfolio.Id,
                UserId = portfolio.UserId,
                Name = portfolio.Name,
                Currency = portfolio.Currency,
                Assets = [.. assets.Where(a => a is not null).Cast<PortfolioAssetDto>()]
            };
        }

        /// <summary>
        /// Существует ли портфель ценных бумаг
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _portfolioRepository.ExistsAsync(id);
        }

        /// <summary>
        /// Редактировать портфель
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <param name="updatingPortfolioDto">DTO для редактирования портфеля</param>
        public async Task UpdateAsync(Guid id, UpdatingPortfolioDto updatingPortfolioDto)
        {
            Portfolio? portfolio = await _portfolioRepository.GetByIdAsync(id);
            if (portfolio is not null)
            {
                portfolio.Name = updatingPortfolioDto.Name;
                portfolio.Currency = updatingPortfolioDto.Currency;
                await _portfolioRepository.UpdateAsync(portfolio);
            }
        }
    }
}
