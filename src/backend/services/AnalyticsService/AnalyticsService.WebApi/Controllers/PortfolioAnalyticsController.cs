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
    /// Контроллер для аналитики портфелей
    /// </summary>
    [ApiController]
    [Route("api/analytics/portfolios")]
    [Produces("application/json")]
    [Authorize]
    [OpenApiTag("Portfolio Analytics")]
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
        /// Получить историю транзакций портфеля
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>История транзакций портфеля</returns>
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
        /// Сравнить несколько портфелей
        /// </summary>
        /// <param name="request">Параметры запроса для сравнения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат сравнения портфелей</returns>
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

