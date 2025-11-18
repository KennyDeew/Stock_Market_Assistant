using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для редактирования транзакции по активу портфеля
    /// </summary>
    /// <param name="TransactionType">
    /// Тип операции с активом:
    /// - Buy: Покупка (значение: 1)
    /// - Sell: Продажа (значение: 2) 
    /// </param>
    /// <param name="PricePerUnit">Цена за единицу</param>
    /// <param name="Quantity">Количество, шт.</param>
    /// <param name="TransactionDate">Дата операции с активом</param>
    /// <param name="Currency">Валюта операции (RUB, USD и т.д.)</param>
    public record UpdatePortfolioAssetTransactionRequest(
        [EnumDataType(typeof(PortfolioAssetTransactionType))]
        PortfolioAssetTransactionType TransactionType,
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше нуля")]
        decimal PricePerUnit,
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше нуля")]
        int Quantity,
        DateTime TransactionDate,
        [StringLength(10, MinimumLength = 3)]
        string Currency);
}
