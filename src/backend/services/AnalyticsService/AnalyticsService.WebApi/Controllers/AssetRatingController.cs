using Microsoft.AspNetCore.Mvc;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;

namespace StockMarketAssistant.AnalyticsService.WebApi.Controllers
{
    /// <summary>
    /// Контроллер для работы с рейтингом активов
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AssetRatingController : ControllerBase
    {
        private readonly IAssetRatingService _assetRatingService;
        private readonly ILogger<AssetRatingController> _logger;

        public AssetRatingController(IAssetRatingService assetRatingService, ILogger<AssetRatingController> logger)
        {
            _assetRatingService = assetRatingService;
            _logger = logger;
        }

        /// <summary>
        /// Получает рейтинг активов по заданным критериям
        /// </summary>
        /// <param name="request">Параметры запроса рейтинга</param>
        /// <returns>Список рейтингов активов</returns>
        /// <response code="200">Рейтинги успешно получены</response>
        /// <response code="400">Некорректные параметры запроса</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost("get-ratings")]
        [ProducesResponseType(typeof(IEnumerable<AssetRatingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<AssetRatingDto>>> GetAssetRatings([FromBody] AssetRatingRequestDto request)
        {
            try
            {
                _logger.LogInformation("Получение рейтинга активов для периода {PeriodStart} - {PeriodEnd}",
                    request.PeriodStart, request.PeriodEnd);

                var ratings = await _assetRatingService.GetAssetRatingsAsync(request);
                return Ok(ratings);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса рейтинга");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рейтинга активов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получает топ самых покупаемых активов за период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="limit">Количество записей (по умолчанию 10)</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список самых покупаемых активов</returns>
        /// <response code="200">Топ активов успешно получен</response>
        /// <response code="400">Некорректные параметры запроса</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("top-buying")]
        [ProducesResponseType(typeof(IEnumerable<AssetRatingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<AssetRatingDto>>> GetTopBuyingAssets(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] int limit = 10,
            [FromQuery] Guid? portfolioId = null)
        {
            try
            {
                _logger.LogInformation("Получение топ {Limit} покупаемых активов для периода {PeriodStart} - {PeriodEnd}",
                    limit, periodStart, periodEnd);

                var assets = await _assetRatingService.GetTopBuyingAssetsAsync(periodStart, periodEnd, limit, portfolioId);
                return Ok(assets);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса топ покупаемых активов");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении топ покупаемых активов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получает топ самых продаваемых активов за период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="limit">Количество записей (по умолчанию 10)</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Список самых продаваемых активов</returns>
        /// <response code="200">Топ активов успешно получен</response>
        /// <response code="400">Некорректные параметры запроса</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("top-selling")]
        [ProducesResponseType(typeof(IEnumerable<AssetRatingDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<AssetRatingDto>>> GetTopSellingAssets(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] int limit = 10,
            [FromQuery] Guid? portfolioId = null)
        {
            try
            {
                _logger.LogInformation("Получение топ {Limit} продаваемых активов для периода {PeriodStart} - {PeriodEnd}",
                    limit, periodStart, periodEnd);

                var assets = await _assetRatingService.GetTopSellingAssetsAsync(periodStart, periodEnd, limit, portfolioId);
                return Ok(assets);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса топ продаваемых активов");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении топ продаваемых активов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Пересчитывает рейтинги за указанный период
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Результат пересчета</returns>
        /// <response code="200">Рейтинги успешно пересчитаны</response>
        /// <response code="400">Некорректные параметры запроса</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost("recalculate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> RecalculateRatings(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] Guid? portfolioId = null)
        {
            try
            {
                _logger.LogInformation("Пересчет рейтингов для периода {PeriodStart} - {PeriodEnd}",
                    periodStart, periodEnd);

                await _assetRatingService.RecalculateRatingsAsync(periodStart, periodEnd, portfolioId);
                return Ok(new { message = "Рейтинги успешно пересчитаны" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса пересчета рейтингов");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при пересчете рейтингов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получает статистику по рейтингам активов
        /// </summary>
        /// <param name="periodStart">Начало периода</param>
        /// <param name="periodEnd">Конец периода</param>
        /// <param name="portfolioId">Идентификатор портфеля (опционально)</param>
        /// <returns>Статистика рейтингов</returns>
        /// <response code="200">Статистика успешно получена</response>
        /// <response code="400">Некорректные параметры запроса</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<object>> GetStatistics(
            [FromQuery] DateTime periodStart,
            [FromQuery] DateTime periodEnd,
            [FromQuery] Guid? portfolioId = null)
        {
            try
            {
                _logger.LogInformation("Получение статистики рейтингов для периода {PeriodStart} - {PeriodEnd}",
                    periodStart, periodEnd);

                // Получаем топ активы по разным критериям
                var topBuying = await _assetRatingService.GetTopBuyingAssetsAsync(periodStart, periodEnd, 5, portfolioId);
                var topSelling = await _assetRatingService.GetTopSellingAssetsAsync(periodStart, periodEnd, 5, portfolioId);

                var statistics = new
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    PortfolioId = portfolioId,
                    TopBuyingAssets = topBuying,
                    TopSellingAssets = topSelling,
                    GeneratedAt = DateTime.UtcNow
                };

                return Ok(statistics);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Некорректные параметры запроса статистики");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики рейтингов");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }
    }
}
