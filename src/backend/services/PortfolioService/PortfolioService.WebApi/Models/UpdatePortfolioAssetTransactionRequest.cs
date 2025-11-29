using StockMarketAssistant.PortfolioService.Domain.Enums;

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
        PortfolioAssetTransactionType TransactionType,
        decimal PricePerUnit,
        int Quantity,
        DateTime TransactionDate,
        string Currency);
}
