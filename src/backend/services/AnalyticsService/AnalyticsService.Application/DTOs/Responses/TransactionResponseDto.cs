using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Responses;

/// <summary>
/// DTO ответа для транзакции
/// </summary>
public class TransactionResponseDto
{
    /// <summary>
    /// Идентификатор транзакции
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор портфеля
    /// </summary>
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// Идентификатор актива (StockCardId)
    /// </summary>
    public Guid StockCardId { get; set; }

    /// <summary>
    /// Тип актива
    /// </summary>
    public AssetType AssetType { get; set; }

    /// <summary>
    /// Тип транзакции (покупка/продажа)
    /// </summary>
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Количество активов
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Цена за единицу актива
    /// </summary>
    public decimal PricePerUnit { get; set; }

    /// <summary>
    /// Общая стоимость транзакции
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Время транзакции
    /// </summary>
    public DateTime TransactionTime { get; set; }

    /// <summary>
    /// Валюта транзакции
    /// </summary>
    public string Currency { get; set; } = "RUB";
}

/// <summary>
/// DTO ответа для списка транзакций
/// </summary>
public class TransactionsListResponseDto
{
    /// <summary>
    /// Список транзакций
    /// </summary>
    public List<TransactionResponseDto> Transactions { get; set; } = new();

    /// <summary>
    /// Общее количество транзакций
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Начальная дата периода
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Конечная дата периода
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Тип транзакции (если был указан фильтр)
    /// </summary>
    public TransactionType? FilteredTransactionType { get; set; }
}

