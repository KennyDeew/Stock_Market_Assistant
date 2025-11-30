using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для создания портфеля ценных бумаг
    /// </summary>
    public record CreatingPortfolioDto(Guid UserId, [MaxLength(100)] string Name, [MaxLength(10)] string Currency = "RUB", bool IsPrivate = false);
}
