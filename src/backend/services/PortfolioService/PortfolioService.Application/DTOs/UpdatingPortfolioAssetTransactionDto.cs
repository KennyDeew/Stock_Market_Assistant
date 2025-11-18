using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для обновления транзакции по активу портфеля
    /// </summary>
    public record UpdatingPortfolioAssetTransactionDto
    (
        PortfolioAssetTransactionType TransactionType,
        int Quantity,
        decimal PricePerUnit,
        DateTime TransactionDate,
        [MaxLength(10)]
        string Currency);
}
