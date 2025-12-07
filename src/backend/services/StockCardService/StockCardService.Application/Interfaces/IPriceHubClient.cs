using StockMarketAssistant.StockCardService.Application.DTOs;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    public interface IPriceHubClient
    {
        Task SendPriceUpdateAsync(string ticker, StockPriceDto stockPrice, CancellationToken token = default);
    }
}
