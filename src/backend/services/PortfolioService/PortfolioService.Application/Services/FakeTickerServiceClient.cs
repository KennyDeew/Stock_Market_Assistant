using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;

namespace StockMarketAssistant.PortfolioService.Application.Services
{
    // Реализация заглушки
    public class FakeStockCardServiceClient : IStockCardServiceClient

    {
        public Task<StockCardInfoDto> GetStockCardInfoAsync(Guid id)
        {
            return Task.FromResult(new StockCardInfoDto($"FAKE_TICKER_FOR_{id}", $"FAKE_NAME_FOR_{id}", $"FAKE_DESC_FOR_{id}"));
        }
    }
}
