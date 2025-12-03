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
    /// Аналитика активов
    /// </summary>
    [ApiController]
    [Route("api/analytics/assets")]
    [Produces("application/json")]
    [Authorize]
    [OpenApiTag("Asset Analytics", Description = "Операции для получения аналитики по активам: топ активов по покупкам и продажам")]
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
        /// Получить топ активов по количеству покупок за указанный период
        /// </summary>
        /// <param name="request">Параметры запроса: количество активов в топе (Top), начальная и конечная даты периода (StartDate, EndDate), контекст анализа (Global или Portfolio), идентификатор портфеля (PortfolioId, опционально для Portfolio контекста)</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Топ активов по количеству покупок с детальной статистикой</returns>
        /// <remarks>
        /// Возвращает список наиболее часто покупаемых активов за указанный период.
        /// Ранжирование выполняется по количеству транзакций покупки.
        ///
        /// Контексты анализа:
        /// - Global: аналитика по всем портфелям системы
        /// - Portfolio: аналитика по конкретному портфелю (требует указания PortfolioId)
        ///
        /// Пример запроса (глобальный контекст):
        /// GET /api/analytics/assets/top-bought?top=10&amp;startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z&amp;context=Global
        ///
        /// Пример запроса (контекст портфеля):
        /// GET /api/analytics/assets/top-bought?top=10&amp;startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z&amp;context=Portfolio&amp;portfolioId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///
        /// Пример ответа:
        /// {
        ///   "assets": [
        ///     {
        ///       "stockCardId": "...",
        ///       "ticker": "SBER",
        ///       "name": "Сбербанк",
        ///       "buyCount": 150,
        ///       "totalBuyAmount": 1000000.00,
        ///       ...
        ///     }
        ///   ],
        ///   "top": 10,
        ///   "startDate": "2024-01-01T00:00:00Z",
        ///   "endDate": "2024-01-31T23:59:59Z",
        ///   "context": "Global"
        /// }
        /// </remarks>
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
        /// Получить топ активов по количеству продаж за указанный период
        /// </summary>
        /// <param name="request">Параметры запроса: количество активов в топе (Top), начальная и конечная даты периода (StartDate, EndDate), контекст анализа (Global или Portfolio), идентификатор портфеля (PortfolioId, опционально для Portfolio контекста)</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Топ активов по количеству продаж с детальной статистикой</returns>
        /// <remarks>
        /// Возвращает список наиболее часто продаваемых активов за указанный период.
        /// Ранжирование выполняется по количеству транзакций продажи.
        ///
        /// Контексты анализа:
        /// - Global: аналитика по всем портфелям системы
        /// - Portfolio: аналитика по конкретному портфелю (требует указания PortfolioId)
        ///
        /// Пример запроса (глобальный контекст):
        /// GET /api/analytics/assets/top-sold?top=10&amp;startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z&amp;context=Global
        ///
        /// Пример запроса (контекст портфеля):
        /// GET /api/analytics/assets/top-sold?top=10&amp;startDate=2024-01-01T00:00:00Z&amp;endDate=2024-01-31T23:59:59Z&amp;context=Portfolio&amp;portfolioId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        ///
        /// Пример ответа:
        /// {
        ///   "assets": [
        ///     {
        ///       "stockCardId": "...",
        ///       "ticker": "GAZP",
        ///       "name": "Газпром",
        ///       "sellCount": 120,
        ///       "totalSellAmount": 800000.00,
        ///       ...
        ///     }
        ///   ],
        ///   "top": 10,
        ///   "startDate": "2024-01-01T00:00:00Z",
        ///   "endDate": "2024-01-31T23:59:59Z",
        ///   "context": "Global"
        /// }
        /// </remarks>
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

