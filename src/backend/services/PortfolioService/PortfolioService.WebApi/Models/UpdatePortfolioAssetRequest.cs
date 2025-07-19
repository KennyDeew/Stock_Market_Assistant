namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    public record UpdatePortfolioAssetRequest(int Quantity, decimal AveragePurchasePrice, DateTime LastUpdated, string Currency);
}