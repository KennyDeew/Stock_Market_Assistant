using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для создания финансового актива в портфеле ценных бумаг
    /// </summary>
    /// <param name="PortfolioId">Идентификатор портфеля-владельца актива</param>
    /// <param name="StockCardId">Идентификатор карточки актива</param>
    /// <param name="AssetType">Тип актива</param>
    /// <param name="PurchasePricePerUnit">Цена покупки за единицу актива</param>
    /// <param name="Quantity">Количество, шт. (по умолчанию 1)</param>
    public record CreatingPortfolioAssetDto(Guid PortfolioId, Guid StockCardId, PortfolioAssetType AssetType, decimal PurchasePricePerUnit, int Quantity);
}
