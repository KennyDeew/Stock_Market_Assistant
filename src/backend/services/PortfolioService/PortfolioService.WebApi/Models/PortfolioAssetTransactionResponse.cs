using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.Text.Json.Serialization;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для возвращаемого значения по транзакции актива портфеля
    /// </summary>
    /// <param name="Id">Уникальный идентификатор транзакции</param>
    /// <param name="PortfolioAssetId">Идентификатор актива портфеля</param>
    /// <param name="Quantity">Количество</param>
    /// <param name="PricePerUnit">Цена за единицу</param>
    /// <param name="Currency">Валюта транзакции (RUB, USD и т.д.)</param>
    /// <param name="TransactionDate">Дата транзакции</param>
    public record PortfolioAssetTransactionResponse(Guid Id,
        Guid PortfolioAssetId,
        int Quantity,
        decimal PricePerUnit,
        string Currency,
        DateTime TransactionDate)
    {
        /// <summary>
        /// Тип транзакции актива портфеля
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PortfolioAssetTransactionType TransactionType { get; init; }

    };
}
