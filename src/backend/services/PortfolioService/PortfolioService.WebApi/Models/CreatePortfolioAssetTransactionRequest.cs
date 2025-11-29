using StockMarketAssistant.PortfolioService.Domain.Enums;

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
        PortfolioAssetTransactionType TransactionType,
        int Quantity,
        decimal PricePerUnit,
        DateTime TransactionDate);
}