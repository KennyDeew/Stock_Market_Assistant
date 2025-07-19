using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для редактирования актива портфеля ценных бумаг
    /// </summary>
    public record UpdatingPortfolioAssetDto(int Quantity, decimal AveragePurchasePrice, DateTime LastUpdated, [MaxLength(10)] string Currency);
}