using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для создаваемого в портфеле финансового актива
    /// </summary>
    /// <param name="PortfolioId">Идентификатор портфеля-владельца актива</param>
    /// <param name="StockCardId">Идентификатор карточки ценной бумаги</param>
    /// <param name="AssetType">
    /// Тип актива:
    /// - Share: Акция (значение: 1)
    /// - Bond: Облигация (значение: 2) 
    /// - Crypto: Криптовалюта (значение: 3)
    /// </param>
    /// <param name="Quantity">Количество, шт.</param>
    /// <param name="PurchasePricePerUnit">Цена покупки за единицу актива</param>
    public record CreatePortfolioAssetRequest(
    Guid PortfolioId,
    Guid StockCardId,
    PortfolioAssetType AssetType,
    decimal PurchasePricePerUnit,
    int Quantity = 1);
}
