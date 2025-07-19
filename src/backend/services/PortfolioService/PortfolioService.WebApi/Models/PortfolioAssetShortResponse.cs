namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    public record PortfolioAssetShortResponse(Guid Id, string Ticker, int Quantity = 0, decimal AveragePurchasePrice = 0);
}
