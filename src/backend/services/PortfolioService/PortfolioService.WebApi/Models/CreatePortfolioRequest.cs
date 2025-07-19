namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    public record CreatePortfolioRequest(Guid UserId, string Name, string Currency);
}
