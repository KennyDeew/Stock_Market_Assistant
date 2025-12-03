namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// Сервис для Мосбиржи
    /// </summary>
    public interface IPriceService
    {
        Task<PriceData?> GetPriceForTickerAsync(string ticker);
    }

    public record PriceData(decimal Price, long Volume, int NumTrades);
}
