using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    public record AlertDto(
        Guid Id,
        Guid StockCardId,
        string AssetTicker,
        string AssetName,
        PortfolioAssetType AssetType,
        decimal TargetPrice,
        string AssetCurrency,
        AlertCondition Condition,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateTime? TriggeredAt,
        Guid UserId,
        DateTime? LastChecked);
}
