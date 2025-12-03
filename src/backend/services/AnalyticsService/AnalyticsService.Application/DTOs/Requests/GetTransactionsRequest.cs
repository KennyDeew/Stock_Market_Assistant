using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;

/// <summary>
/// DTO запроса для получения всех сделок за период
/// </summary>
public class GetTransactionsRequest
{
    /// <summary>
    /// Тип периода (сегодня, неделя, месяц, произвольный)
    /// </summary>
    public PeriodType PeriodType { get; set; } = PeriodType.Week;

    /// <summary>
    /// Начальная дата периода (используется только для PeriodType.Custom)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Конечная дата периода (используется только для PeriodType.Custom)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Тип транзакции (покупка/продажа). Если null, возвращаются все типы
    /// </summary>
    public TransactionType? TransactionType { get; set; }
}

/// <summary>
/// Тип периода для выборки транзакций
/// </summary>
public enum PeriodType
{
    /// <summary>
    /// Сегодня
    /// </summary>
    Today = 1,

    /// <summary>
    /// За неделю (по умолчанию)
    /// </summary>
    Week = 2,

    /// <summary>
    /// За месяц
    /// </summary>
    Month = 3,

    /// <summary>
    /// Произвольный период (требуются StartDate и EndDate)
    /// </summary>
    Custom = 4
}

