using StockMarketAssistant.PortfolioService.Application.DTOs;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces
{
    public interface IStockCardServiceClient
    {
        Task<StockCardInfoDto> GetStockCardInfoAsync(Guid id);
    }
}
