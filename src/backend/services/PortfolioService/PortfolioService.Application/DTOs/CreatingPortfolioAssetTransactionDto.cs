using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для создания транзакции по активу портфеля
    /// </summary>
    public record CreatingPortfolioAssetTransactionDto(
        /// <summary>
        /// Тип операции: покупка или продажа
        /// </summary>
        PortfolioAssetTransactionType TransactionType,

        /// <summary>
        /// Количество единиц актива для покупки/продажи
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным числом")]
        int Quantity,

        /// <summary>
        /// Цена за единицу актива
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительным числом")]
        decimal PricePerUnit,

        /// <summary>
        /// Дата и время совершения транзакции (по умолчанию - текущее время)
        /// </summary>
        DateTime? TransactionDate = null,

        /// <summary>
        /// Валюта транзакции (по умолчанию - валюта актива)
        /// </summary>
        [MaxLength(10)]
        string? Currency = null);
}
