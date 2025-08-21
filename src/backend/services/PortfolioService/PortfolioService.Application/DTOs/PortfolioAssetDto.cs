using StockMarketAssistant.PortfolioService.Domain;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для актива ценной бумаги портфеля
    /// </summary>
    public record PortfolioAssetDto(Guid Id, Guid PortfolioId, Guid StockCardId, PortfolioAssetType AssetType, string Ticker, string Name, string Description, int Quantity, decimal AveragePurchasePrice, DateTime LastUpdated, string Currency);
}
