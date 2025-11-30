using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для редактирования портфеля ценных бумаг
    /// </summary>
    public record UpdatingPortfolioDto([MaxLength(100)] string Name, [MaxLength(10)] string Currency, bool IsPrivate);
}
