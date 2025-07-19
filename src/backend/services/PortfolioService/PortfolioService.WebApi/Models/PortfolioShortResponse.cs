namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    public record PortfolioShortResponse(Guid Id, Guid UserId, string Name, string Currency);
}
