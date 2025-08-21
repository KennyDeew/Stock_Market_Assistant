using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    /// <summary>
    /// Сервис работы с финансовыми активами в портфеле
    /// </summary>
    public class PortfolioAssetAppService(IPortfolioAssetRepository portfolioAssetRepository, IStockCardServiceClient stockCardServiceClient) : IPortfolioAssetAppService
    {
        private readonly IPortfolioAssetRepository _portfolioAssetRepository = portfolioAssetRepository;
        private readonly IStockCardServiceClient _stockCardServiceClient = stockCardServiceClient;

        /// <summary>
        /// Создать актив ценной бумаги в портфеле
        /// </summary>
        /// <param name="creatingPortfolioAssetDto">DTO создаваемого актива</param>
        /// <returns>Идентификатор созданного актива</returns>
        public async Task<Guid> CreateAsync(CreatingPortfolioAssetDto creatingPortfolioAssetDto)
        {
            PortfolioAsset portfolioAsset = new(Guid.NewGuid(), creatingPortfolioAssetDto.PortfolioId, creatingPortfolioAssetDto.StockCardId, creatingPortfolioAssetDto.AssetType);
            PortfolioAsset createdPortfolio = await _portfolioAssetRepository.AddAsync(portfolioAsset);
            return createdPortfolio.Id;
        }

        /// <summary>
        /// Удалить актив ценной бумаги в портфеле
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            PortfolioAsset? portfolioAsset = await _portfolioAssetRepository.GetByIdAsync(id);
            if (portfolioAsset is not null)
            {
                await _portfolioAssetRepository.DeleteAsync(portfolioAsset);
            }
        }

        /// <summary>
        /// Получить актив ценной бумаги из портфеля
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns></returns>
        public async Task<PortfolioAssetDto?> GetByIdAsync(Guid id)
        {
            PortfolioAsset? portfolioAsset = await _portfolioAssetRepository.GetByIdAsync(id);
            if (portfolioAsset is null)
                return null;
            // Вызвать внешний микросервис карточки актива
            var stockCardInfo = await _stockCardServiceClient.GetStockCardInfoAsync(portfolioAsset.StockCardId);
            return new PortfolioAssetDto(portfolioAsset.Id, portfolioAsset.PortfolioId, portfolioAsset.StockCardId, portfolioAsset.AssetType, stockCardInfo.Ticker, stockCardInfo.Name, stockCardInfo.Description, portfolioAsset.Quantity, portfolioAsset.AveragePurchasePrice ?? 0, portfolioAsset.LastUpdated ?? DateTime.MinValue, portfolioAsset.Currency ?? string.Empty);
        }

        /// <summary>
        /// Существует ли актив ценной бумаги
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _portfolioAssetRepository.ExistsAsync(id);
        }

        /// <summary>
        /// Редактировать актив ценной бумаги в портфеле
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <param name="updatingPortfolioDto">DTO для редактирования портфеля</param>
        public async Task UpdateAsync(Guid id, UpdatingPortfolioAssetDto updatingPortfolioDto)
        {
            PortfolioAsset? portfolioAsset = await _portfolioAssetRepository.GetByIdAsync(id);
            if (portfolioAsset is not null)
            {
                portfolioAsset.Quantity = updatingPortfolioDto.Quantity;
                portfolioAsset.Currency = updatingPortfolioDto.Currency;
                portfolioAsset.AveragePurchasePrice = updatingPortfolioDto.AveragePurchasePrice;
                portfolioAsset.LastUpdated = updatingPortfolioDto.LastUpdated;
                await _portfolioAssetRepository.UpdateAsync(portfolioAsset);
            }
        }
    }
}
