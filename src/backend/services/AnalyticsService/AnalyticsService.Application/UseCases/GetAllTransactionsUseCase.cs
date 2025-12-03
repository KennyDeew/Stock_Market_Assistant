using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Application.UseCases;

/// <summary>
/// Use Case для получения всех сделок за период с фильтрацией
/// </summary>
public class GetAllTransactionsUseCase
{
    private readonly IAssetTransactionRepository _transactionRepository;
    private readonly ILogger<GetAllTransactionsUseCase> _logger;

    public GetAllTransactionsUseCase(
        IAssetTransactionRepository transactionRepository,
        ILogger<GetAllTransactionsUseCase> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Выполнить use case
    /// </summary>
    public async Task<IEnumerable<AssetTransaction>> ExecuteAsync(
        GetTransactionsRequest request,
        CancellationToken cancellationToken = default)
    {
        // Определяем период на основе типа периода
        Period period;
        var now = DateTime.UtcNow;

        switch (request.PeriodType)
        {
            case PeriodType.Today:
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                period = Period.Custom(todayStart, now);
                break;

            case PeriodType.Week:
                period = Period.LastWeek(now);
                break;

            case PeriodType.Month:
                period = Period.LastMonth(now);
                break;

            case PeriodType.Custom:
                if (!request.StartDate.HasValue || !request.EndDate.HasValue)
                {
                    throw new ArgumentException("Для произвольного периода необходимо указать StartDate и EndDate");
                }
                period = Period.Custom(request.StartDate.Value, request.EndDate.Value);
                break;

            default:
                // По умолчанию - неделя
                period = Period.LastWeek(now);
                break;
        }

        _logger.LogInformation(
            "Получение транзакций за период {Start} - {End}, Тип транзакции: {TransactionType}",
            period.Start, period.End, request.TransactionType?.ToString() ?? "Все");

        // Получаем транзакции через репозиторий
        var transactions = await _transactionRepository.GetByPeriodAndTypeAsync(
            period,
            request.TransactionType,
            cancellationToken);

        _logger.LogInformation("Получено {Count} транзакций", transactions.Count());

        return transactions;
    }
}

