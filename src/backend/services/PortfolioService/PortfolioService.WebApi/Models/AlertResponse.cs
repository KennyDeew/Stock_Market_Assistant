using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.Text.Json.Serialization;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Ответ API — уведомление о цене актива
    /// </summary>
    public record AlertResponse(
        Guid Id,
        Guid StockCardId,
        string Ticker,
        string AssetName,
        decimal TargetPrice,
        string AssetCurrency,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateTime? TriggeredAt,
        string UserId,
        DateTime? LastChecked,
        bool? IsTriggered)
    {
        /// <summary>
        /// Условие срабатывания
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlertCondition Condition { get; init; }
        /// <summary>
        /// Тип актива
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PortfolioAssetType AssetType { get; init; }
    }
}
