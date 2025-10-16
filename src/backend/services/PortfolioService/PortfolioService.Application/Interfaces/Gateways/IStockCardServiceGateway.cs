using StockMarketAssistant.SharedLibrary.Models;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways
{
    public interface IStockCardServiceGateway
    {
        //Task UpdateAllPricesForShareCardsAsync();

        Task<ShareCardShortModel?> GetShortShareCardModelByIdAsync(Guid id);

        Task<BondCardShortModel?> GetShortBondCardModelByIdAsync(Guid id);
    }
}
