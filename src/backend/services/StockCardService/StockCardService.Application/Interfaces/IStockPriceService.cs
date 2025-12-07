using StockMarketAssistant.StockCardService.Application.DTOs;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    public interface IStockPriceService
    {
        Task<decimal?> GetCurrentPriceAsync(string ticker, string market, string board, CancellationToken cancellationToken);
        Task<StockPriceDto> GetStockPricesAsync(string ticker, string market, string board, CancellationToken cancellationToken);
    }
}
