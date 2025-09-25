using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для актива ценной бумаги портфеля
    /// </summary>
    public record PortfolioAssetDto(
        Guid Id,
        Guid PortfolioId,
        Guid StockCardId,
        PortfolioAssetType AssetType,
        string Ticker,
        string Name,
        string Description,
        int TotalQuantity,
        decimal AveragePurchasePrice,
        [MaxLength(10)]
        string Currency,
        DateTime LastUpdated,
        IReadOnlyCollection<PortfolioAssetTransactionDto> Transactions);
}
