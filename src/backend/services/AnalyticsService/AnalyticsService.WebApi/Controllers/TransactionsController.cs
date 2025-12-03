using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Responses;
using StockMarketAssistant.AnalyticsService.Application.UseCases;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.WebApi.Controllers;

/// <summary>
/// Транзакции
/// </summary>
[ApiController]
[Route("api/analytics/transactions")]
[Produces("application/json")]
[Authorize]
[OpenApiTag("Transactions", Description = "Операции для получения информации о транзакциях активов с фильтрацией по периоду и типу")]
public class TransactionsController : ControllerBase
{
    private readonly GetAllTransactionsUseCase _getAllTransactionsUseCase;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        GetAllTransactionsUseCase getAllTransactionsUseCase,
        ILogger<TransactionsController> logger)
    {
        _getAllTransactionsUseCase = getAllTransactionsUseCase ?? throw new ArgumentNullException(nameof(getAllTransactionsUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить все сделки за период с возможностью фильтрации по типу
    /// </summary>
    /// <param name="request">Параметры запроса: период (Today, Week, Month, Custom), тип транзакции (Buy, Sell или null для всех), даты для произвольного периода (StartDate, EndDate)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список транзакций за указанный период с информацией о фильтрах и общем количестве</returns>
    /// <remarks>
    /// Получает список всех транзакций активов за указанный период с возможностью фильтрации по типу транзакции (покупка/продажа).
    ///
    /// Поддерживаемые типы периодов:
    /// - Today: транзакции за сегодня
    /// - Week: транзакции за последние 7 дней (по умолчанию)
    /// - Month: транзакции за последний месяц
    /// - Custom: произвольный период (требует указания StartDate и EndDate)
    ///
    /// Типы транзакций:
    /// - Buy: только покупки
    /// - Sell: только продажи
    /// - null: все транзакции
    ///
    /// Пример запроса за неделю:
    /// GET /api/analytics/transactions?periodType=Week
    ///
    /// Пример запроса за месяц с фильтром по покупкам:
    /// GET /api/analytics/transactions?periodType=Month&amp;transactionType=Buy
    ///
    /// Пример запроса за произвольный период:
    /// GET /api/analytics/transactions?periodType=Custom&amp;startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z
    ///
    /// Пример ответа:
    /// {
    ///   "transactions": [...],
    ///   "totalCount": 150,
    ///   "startDate": "2024-01-01T00:00:00Z",
    ///   "endDate": "2024-01-31T23:59:59Z",
    ///   "filteredTransactionType": "Buy"
    /// }
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(TransactionsListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransactionsListResponseDto>> GetAllTransactions(
        [FromQuery] GetTransactionsRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Используем значения по умолчанию, если запрос не передан
            request ??= new GetTransactionsRequest
            {
                PeriodType = PeriodType.Week, // По умолчанию - неделя
                TransactionType = null // По умолчанию - все типы
            };

            // Валидация для произвольного периода
            if (request.PeriodType == PeriodType.Custom)
            {
                if (!request.StartDate.HasValue || !request.EndDate.HasValue)
                {
                    return BadRequest(new { error = "BadRequest", message = "Для произвольного периода необходимо указать StartDate и EndDate" });
                }

                if (request.StartDate.Value >= request.EndDate.Value)
                {
                    return BadRequest(new { error = "BadRequest", message = "Начальная дата должна быть меньше конечной даты" });
                }
            }

            _logger.LogInformation(
                "Запрос всех транзакций. Тип периода: {PeriodType}, Тип транзакции: {TransactionType}",
                request.PeriodType, request.TransactionType?.ToString() ?? "Все");

            // Выполняем use case
            var transactions = await _getAllTransactionsUseCase.ExecuteAsync(request, cancellationToken);

            // Определяем период для ответа
            var now = DateTime.UtcNow;
            DateTime startDate, endDate;

            switch (request.PeriodType)
            {
                case PeriodType.Today:
                    startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                    endDate = now;
                    break;

                case PeriodType.Week:
                    startDate = now.AddDays(-7);
                    endDate = now;
                    break;

                case PeriodType.Month:
                    startDate = now.AddMonths(-1);
                    endDate = now;
                    break;

                case PeriodType.Custom:
                    startDate = request.StartDate!.Value;
                    endDate = request.EndDate!.Value;
                    break;

                default:
                    startDate = now.AddDays(-7);
                    endDate = now;
                    break;
            }

            // Преобразуем в DTO
            var response = new TransactionsListResponseDto
            {
                Transactions = transactions.Select(t => new TransactionResponseDto
                {
                    Id = t.Id,
                    PortfolioId = t.PortfolioId,
                    StockCardId = t.StockCardId,
                    AssetType = t.AssetType,
                    TransactionType = t.TransactionType,
                    Quantity = t.Quantity,
                    PricePerUnit = t.PricePerUnit,
                    TotalAmount = t.TotalAmount,
                    TransactionTime = t.TransactionTime,
                    Currency = t.Currency
                }).ToList(),
                TotalCount = transactions.Count(),
                StartDate = startDate,
                EndDate = endDate,
                FilteredTransactionType = request.TransactionType
            };

            _logger.LogInformation("Возвращено {Count} транзакций", response.TotalCount);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Некорректные параметры запроса для получения транзакций");
            return BadRequest(new { error = "BadRequest", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении транзакций");
            throw;
        }
    }
}

