using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Mappings
{
    /// <summary>
    /// Маппинг между DTO приложения и моделями контроллера для уведомлений о ценах активов
    /// </summary>
    public static class AlertMappingcs
    {
        /// <summary>
        /// Преобразование DTO уведомлений о ценах активов в модель ответа
        /// </summary>        
        public static AlertResponse ToResponse(this AlertDto dto)
        {
            return new AlertResponse(
                Id: dto.Id,
                StockCardId: dto.StockCardId,
                Ticker: dto.AssetTicker,
                AssetName: dto.AssetName,
                TargetPrice: dto.TargetPrice,
                AssetCurrency: dto.AssetCurrency,
                IsActive: dto.IsActive,
                CreatedAt: dto.CreatedAt,
                UpdatedAt: dto.UpdatedAt,
                TriggeredAt: dto.TriggeredAt,
                UserId: dto.UserId.ToString(),
                LastChecked: dto.LastChecked,
                IsTriggered: dto.TriggeredAt.HasValue ? true : null
            )
            {
                Condition = dto.Condition,
                AssetType = dto.AssetType
            };
        }
    }
}
