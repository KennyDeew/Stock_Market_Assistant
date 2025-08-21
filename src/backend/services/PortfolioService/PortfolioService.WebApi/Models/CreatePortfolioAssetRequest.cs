using StockMarketAssistant.PortfolioService.Domain;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    public record CreatePortfolioAssetRequest(Guid PortfolioId, Guid StockCardId, PortfolioAssetType AssetType);
}