using StockMarketAssistant.SharedLibrary.Models;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways
{
    public interface IStockCardServiceGateway
    {
        Task UpdateAllPricesForShareCardsAsync();

        Task UpdateAllPricesForBondCardsAsync();

        Task<ShareCardShortModel?> GetShortShareCardModelByIdAsync(Guid id);

        Task<BondCardShortModel?> GetShortBondCardModelByIdAsync(Guid id);
    }
}
