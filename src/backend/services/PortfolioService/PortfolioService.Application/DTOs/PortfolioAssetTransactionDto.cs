using StockMarketAssistant.PortfolioService.Domain.Enums;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для представления транзакции актива портфеля
    /// </summary>
    /// <param name="Id">Уникальный идентификатор транзакции</param>
    /// <param name="PortfolioAssetId">Идентификатор актива портфеля, к которому относится транзакция</param>
    /// <param name="TransactionDate">Дата и время совершения транзакции</param>
    /// <param name="TransactionType">Тип транзакции (покупка/продажа)</param>
    /// <param name="Quantity">Количество единиц актива в транзакции</param>
    /// <param name="PricePerUnit">Цена за единицу актива в транзакции</param>
    /// <param name="Currency">Валюта транзакции</param>
    public record PortfolioAssetTransactionDto(
        Guid Id,
        Guid PortfolioAssetId,
        DateTime TransactionDate,
        PortfolioAssetTransactionType TransactionType,
        int Quantity,
        decimal PricePerUnit,
        string Currency)
    {
        /// <summary>
        /// Общая сумма транзакции (вычисляемое поле)
        /// </summary>
        public decimal TotalAmount => Quantity * PricePerUnit;
    };
}

