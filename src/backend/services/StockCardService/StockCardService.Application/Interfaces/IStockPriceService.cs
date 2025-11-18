namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    public interface IStockPriceService
    {
        Task<decimal?> GetCurrentPriceAsync(string ticker, string market, string board, CancellationToken cancellationToken);
    }
}
