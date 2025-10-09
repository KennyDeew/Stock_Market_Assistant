using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

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
    [Required]
    Guid PortfolioId,
    [Required]
    Guid StockCardId,
    [EnumDataType(typeof(PortfolioAssetType))]
    PortfolioAssetType AssetType,
    [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
    decimal PurchasePricePerUnit,
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
    int Quantity = 1);
}
