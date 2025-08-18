using StockMarketAssistant.PortfolioService.Domain;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    public record PortfolioAssetResponse(Guid Id, Guid PortfolioId, Guid StockCardId, string AssetType, string Ticker, string Name, string Description, int Quantity, decimal AveragePurchasePrice, DateTime LastUpdated, string Currency);
}