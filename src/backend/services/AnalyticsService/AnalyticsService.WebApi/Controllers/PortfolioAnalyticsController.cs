using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Responses;
using StockMarketAssistant.AnalyticsService.Application.UseCases;

namespace StockMarketAssistant.AnalyticsService.WebApi.Controllers
{
    /// <summary>
    /// Аналитика портфелей
    /// </summary>
    [ApiController]
    [Route("api/analytics/portfolios")]
    [Produces("application/json")]
    [Authorize]
    [OpenApiTag("Portfolio Analytics", Description = "Операции для получения аналитики по портфелям: история транзакций и сравнение портфелей")]
    public class PortfolioAnalyticsController : ControllerBase
    {
        private readonly GetPortfolioHistoryUseCase _getPortfolioHistoryUseCase;
        private readonly ComparePortfoliosUseCase _comparePortfoliosUseCase;
        private readonly ILogger<PortfolioAnalyticsController> _logger;

        public PortfolioAnalyticsController(
            GetPortfolioHistoryUseCase getPortfolioHistoryUseCase,
            ComparePortfoliosUseCase comparePortfoliosUseCase,
            ILogger<PortfolioAnalyticsController> logger)
        {
            _getPortfolioHistoryUseCase = getPortfolioHistoryUseCase ?? throw new ArgumentNullException(nameof(getPortfolioHistoryUseCase));
            _comparePortfoliosUseCase = comparePortfoliosUseCase ?? throw new ArgumentNullException(nameof(comparePortfoliosUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Получить историю транзакций портфеля за указанный период
        /// </summary>
        /// <param name="id">Уникальный идентификатор портфеля</param>
        /// <param name="startDate">Начальная дата периода (включительно) в формате UTC</param>
        /// <param name="endDate">Конечная дата периода (включительно) в формате UTC</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>История транзакций портфеля с детализацией по активам и транзакциям</returns>
        /// <remarks>
        /// Возвращает полную историю транзакций портфеля за указанный период времени.
        /// Включает информацию о всех активах портфеля и их транзакциях (покупки и продажи).
        ///
        /// Пример запроса:
        /// GET /api/analytics/portfolios/3fa85f64-5717-4562-b3fc-2c963f66afa6/history?startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z
        ///
        /// Пример ответа:
        /// {
        ///   "portfolioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "startDate": "2024-01-01T00:00:00Z",
        ///   "endDate": "2024-01-31T23:59:59Z",
        ///   "assets": [
        ///     {
        ///       "stockCardId": "...",
        ///       "ticker": "SBER",
        ///       "name": "Сбербанк",
        ///       "transactions": [...]
        ///     }
        ///   ]
        /// }
        /// </remarks>
        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(typeof(PortfolioHistoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioHistoryResponseDto>> GetPortfolioHistory(
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Запрос истории портфеля {PortfolioId} за период {StartDate} - {EndDate}",
                    id, startDate, endDate);

                var history = await _getPortfolioHistoryUseCase.ExecuteAsync(
                    id,
                    startDate,
                    endDate,
                    cancellationToken);

                if (history == null)
                {
                    return NotFound(new { error = "NotFound", message = $"Портфель с ID {id} не найден" });
                }

                var response = PortfolioHistoryResponseDtoExtensions.FromPortfolioHistoryDto(history);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса для истории портфеля {PortfolioId}", id);
                return BadRequest(new { error = "BadRequest", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении истории портфеля {PortfolioId}", id);
                throw;
            }
        }

        /// <summary>
        /// Сравнить несколько портфелей за указанный период
        /// </summary>
        /// <param name="request">Параметры запроса для сравнения: список идентификаторов портфелей, начальная и конечная даты периода</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат сравнения портфелей с аналитикой по каждому портфелю</returns>
        /// <remarks>
        /// Сравнивает несколько портфелей по различным метрикам за указанный период времени.
        /// Возвращает детальную аналитику для каждого портфеля, включая:
        /// - Общее количество транзакций
        /// - Количество активов
        /// - Общую стоимость транзакций
        /// - Статистику по покупкам и продажам
        ///
        /// Пример запроса:
        /// POST /api/analytics/portfolios/compare
        /// Content-Type: application/json
        /// {
        ///   "portfolioIds": [
        ///     "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "7fa85f64-5717-4562-b3fc-2c963f66afa7"
        ///   ],
        ///   "startDate": "2024-01-01T00:00:00Z",
        ///   "endDate": "2024-01-31T23:59:59Z"
        /// }
        ///
        /// Пример ответа:
        /// {
        ///   "portfolios": [
        ///     {
        ///       "portfolioId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "totalTransactions": 150,
        ///       "totalAssets": 25,
        ///       "totalAmount": 1000000.00,
        ///       ...
        ///     }
        ///   ],
        ///   "startDate": "2024-01-01T00:00:00Z",
        ///   "endDate": "2024-01-31T23:59:59Z"
        /// }
        /// </remarks>
        [HttpPost("compare")]
        [ProducesResponseType(typeof(PortfolioComparisonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioComparisonDto>> ComparePortfolios(
            [FromBody] ComparePortfoliosRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Запрос сравнения {Count} портфелей",
                    request.PortfolioIds.Count);

                var result = await _comparePortfoliosUseCase.ExecuteAsync(
                    request.PortfolioIds,
                    request.StartDate,
                    request.EndDate,
                    cancellationToken);

                var response = PortfolioComparisonDto.FromComparisonResult(
                    result,
                    request.StartDate,
                    request.EndDate);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса для сравнения портфелей");
                return BadRequest(new { error = "BadRequest", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сравнении портфелей");
                throw;
            }
        }
    }
}

