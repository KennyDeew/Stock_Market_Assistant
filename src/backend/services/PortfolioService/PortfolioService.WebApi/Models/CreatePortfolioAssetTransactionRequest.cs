using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для создаваемой в активе портфеля транзакции
    /// </summary>
    /// <param name="TransactionType">
    /// Тип операции с активом:
    /// - Buy: Покупка (значение: 1)
    /// - Sell: Продажа (значение: 2) 
    /// </param>
    /// <param name="Quantity">Количество, шт.</param>
    /// <param name="PricePerUnit">Цена за единицу актива</param>
    /// <param name="TransactionDate">Дата операции с активом</param>
    public record CreatePortfolioAssetTransactionRequest(
        [EnumDataType(typeof(PortfolioAssetTransactionType))]
        PortfolioAssetTransactionType TransactionType,
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
        int Quantity,
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
        decimal PricePerUnit,
        DateTime TransactionDate);
}