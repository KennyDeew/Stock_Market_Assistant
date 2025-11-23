using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Requests;
using StockMarketAssistant.AnalyticsService.Application.DTOs.Responses;
using StockMarketAssistant.AnalyticsService.Application.UseCases;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.WebApi.Controllers
{
    /// <summary>
    /// Контроллер для аналитики активов
    /// </summary>
    [ApiController]
    [Route("api/analytics/assets")]
    [Produces("application/json")]
    [Authorize]
    [OpenApiTag("Asset Analytics")]
    public class AssetAnalyticsController : ControllerBase
    {
        private readonly GetTopBoughtAssetsUseCase _getTopBoughtAssetsUseCase;
        private readonly GetTopSoldAssetsUseCase _getTopSoldAssetsUseCase;
        private readonly ILogger<AssetAnalyticsController> _logger;

        public AssetAnalyticsController(
            GetTopBoughtAssetsUseCase getTopBoughtAssetsUseCase,
            GetTopSoldAssetsUseCase getTopSoldAssetsUseCase,
            ILogger<AssetAnalyticsController> logger)
        {
            _getTopBoughtAssetsUseCase = getTopBoughtAssetsUseCase ?? throw new ArgumentNullException(nameof(getTopBoughtAssetsUseCase));
            _getTopSoldAssetsUseCase = getTopSoldAssetsUseCase ?? throw new ArgumentNullException(nameof(getTopSoldAssetsUseCase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Получить топ активов по количеству покупок
        /// </summary>
        /// <param name="request">Параметры запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Топ активов по покупкам</returns>
        [HttpGet("top-bought")]
        [ProducesResponseType(typeof(TopAssetsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TopAssetsResponseDto>> GetTopBoughtAssets(
            [FromQuery] GetTopAssetsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Запрос топ {Top} активов по покупкам за период {StartDate} - {EndDate}, Context: {Context}",
                    request.Top, request.StartDate, request.EndDate, request.Context);

                var assets = await _getTopBoughtAssetsUseCase.ExecuteAsync(
                    request.StartDate,
                    request.EndDate,
                    request.Top,
                    request.Context,
                    request.PortfolioId,
                    cancellationToken);

                var response = TopAssetsResponseDto.FromEntities(
                    assets,
                    request.StartDate,
                    request.EndDate,
                    request.Context,
                    request.PortfolioId,
                    request.Top);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса для топ активов по покупкам");
                return BadRequest(new { error = "BadRequest", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении топ активов по покупкам");
                throw;
            }
        }

        /// <summary>
        /// Получить топ активов по количеству продаж
        /// </summary>
        /// <param name="request">Параметры запроса</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Топ активов по продажам</returns>
        [HttpGet("top-sold")]
        [ProducesResponseType(typeof(TopAssetsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TopAssetsResponseDto>> GetTopSoldAssets(
            [FromQuery] GetTopAssetsRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Запрос топ {Top} активов по продажам за период {StartDate} - {EndDate}, Context: {Context}",
                    request.Top, request.StartDate, request.EndDate, request.Context);

                var assets = await _getTopSoldAssetsUseCase.ExecuteAsync(
                    request.StartDate,
                    request.EndDate,
                    request.Top,
                    request.Context,
                    request.PortfolioId,
                    cancellationToken);

                var response = TopAssetsResponseDto.FromEntities(
                    assets,
                    request.StartDate,
                    request.EndDate,
                    request.Context,
                    request.PortfolioId,
                    request.Top);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса для топ активов по продажам");
                return BadRequest(new { error = "BadRequest", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении топ активов по продажам");
                throw;
            }
        }
    }
}

