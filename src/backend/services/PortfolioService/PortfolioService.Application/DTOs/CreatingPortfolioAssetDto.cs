using StockMarketAssistant.PortfolioService.Domain;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для создания финансового актива в портфеле ценных бумаг
    /// </summary>
    public record CreatingPortfolioAssetDto(Guid PortfolioId, Guid StockCardId, PortfolioAssetType AssetType);
}
