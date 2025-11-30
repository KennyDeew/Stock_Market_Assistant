using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.WebApi.Mappings;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Controllers
{
    /// <summary>
    /// Портфели
    /// </summary>
    [ApiController]
    //[Route("api/v1/[controller]")]
    [Route("api/v1/portfolios")]
    [Produces("application/json")]
    [OpenApiTag("Portfolios")]
    public class PortfoliosController(IPortfolioAppService portfolioAppService, ILogger<PortfoliosController> logger) : ControllerBase
    {
        private readonly IPortfolioAppService _portfolioAppService = portfolioAppService;
        private readonly ILogger<PortfoliosController> _logger = logger;

        /// <summary>
        /// Получить список всех портфелей от всех пользователей
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1)</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10, максимум 100)</param>
        /// <returns>Пагинированный список портфелей</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<PortfolioShortResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<PortfolioShortResponse>>> GetPortfolios(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogDebug("Получение списка портфелей. Страница: {Page}, Размер: {PageSize}", page, pageSize);

            try
            {
                // Валидация параметров
                if (page < 1)
                {
                    return BadRequest("Номер страницы должен быть больше 0");
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Размер страницы должен быть от 1 до 100");
                }

                var portfoliosDtos = await _portfolioAppService.GetAllAsync();

                // Применяем пагинацию
                var totalItems = portfoliosDtos.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var pagedPortfolios = portfoliosDtos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var portfolioResponses = pagedPortfolios.Select(p =>
                    new PortfolioShortResponse(p.Id, p.UserId, p.Name, p.Currency, p.IsPrivate));

                var paginatedResponse = new PaginatedResponse<PortfolioShortResponse>(
                    portfolioResponses,
                    totalItems,
                    page,
                    pageSize,
                    totalPages);

                _logger.LogDebug("Получено {Count} портфелей из {Total} на странице {Page}",
                                pagedPortfolios.Count, totalItems, page);

                return Ok(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка портфелей");
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить список портфелей конкретного пользователя
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <param name="page">Номер страницы (по умолчанию 1)</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10, максимум 100)</param>
        /// <returns>Пагинированный список портфелей пользователя</returns>
        [Authorize(Roles = "ADMIN,USER")]
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(PaginatedResponse<PortfolioShortResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<PortfolioShortResponse>>> GetUserPortfolios(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogDebug("Получение списка портфелей пользователя {UserId}. Страница: {Page}, Размер: {PageSize}",
                userId, page, pageSize);

            try
            {
                // Валидация параметров
                if (page < 1)
                {
                    return BadRequest("Номер страницы должен быть больше 0");
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Размер страницы должен быть от 1 до 100");
                }

                // Получаем портфели конкретного пользователя
                var portfoliosDtos = await _portfolioAppService.GetByUserIdAsync(userId);

                // Применяем пагинацию
                var totalItems = portfoliosDtos.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var pagedPortfolios = portfoliosDtos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var portfolioResponses = pagedPortfolios.Select(p =>
                    new PortfolioShortResponse(p.Id, p.UserId, p.Name, p.Currency, p.IsPrivate));

                var paginatedResponse = new PaginatedResponse<PortfolioShortResponse>(
                    portfolioResponses,
                    totalItems,
                    page,
                    pageSize,
                    totalPages);

                _logger.LogDebug("Получено {Count} портфелей пользователя {UserId} из {Total} на странице {Page}",
                    pagedPortfolios.Count, userId, totalItems, page);

                return Ok(paginatedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка портфелей пользователя {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Создать новый портфель
        /// </summary>
        /// <param name="request">Параметры создаваемого портфеля</param>
        /// <returns></returns>
        [Authorize(Roles = "USER")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PortfolioShortResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioShortResponse>> CreatePortfolio(CreatePortfolioRequest request)
        {
            try
            {
                CreatingPortfolioDto createDto = new(request.UserId, request.Name, request.Currency, request.IsPrivate);
                var createdPortfolioId = await _portfolioAppService.CreateAsync(createDto);

                if (createdPortfolioId == Guid.Empty)
                {
                    _logger.LogWarning("Портфель для пользователя {UserId} с именем {PortfolioName} не был создан",
                                     request.UserId, request.Name);
                    return BadRequest("Произошла ошибка при создании портфеля");
                }

                var response = new PortfolioShortResponse(
                    createdPortfolioId,
                    createDto.UserId,
                    createDto.Name,
                    createDto.Currency,
                    createDto.IsPrivate);

                _logger.LogInformation("Портфель {PortfolioId} успешно создан для пользователя {UserId}",
                                     createdPortfolioId, request.UserId);

                return CreatedAtAction(
                    nameof(GetPortfolioById),  // Имя метода для получения созданного ресурса
                    new { id = createdPortfolioId },  // Параметры для метода
                    response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при создании портфеля для пользователя {UserId} с именем {PortfolioName}",
                                request.UserId, request.Name);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить данные портфеля по Id
        /// </summary>
        /// <param name="id">Id портфеля</param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN,USER")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioResponse>> GetPortfolioById(Guid id)
        {
            try
            {
                var portfolioDto = await _portfolioAppService.GetByIdAsync(id);
                if (portfolioDto is null)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден", id);
                    return NotFound();
                }

                var portfolioModel = new PortfolioResponse
                {
                    Id = portfolioDto.Id,
                    UserId = portfolioDto.UserId,
                    Name = portfolioDto.Name,
                    Currency = portfolioDto.Currency,
                    IsPrivate = portfolioDto.IsPrivate,
                    Assets = [.. portfolioDto.Assets.Select(a => new PortfolioAssetShortResponse(
                        a.Id,
                        a.PortfolioId,
                        a.StockCardId,
                        a.Ticker,
                        a.Name,
                        a.TotalQuantity,
                        a.AveragePurchasePrice,
                        a.Currency)
                    {
                        AssetType = a.AssetType
                    })]
                };

                return Ok(portfolioModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при получении портфеля с ID {PortfolioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Редактировать портфель
        /// </summary>
        /// <param name="id">Id портфеля</param>
        /// <param name="request">Данные для редактирования портфеля</param>
        [Authorize(Roles = "USER")]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePortfolio(Guid id, [FromBody] UpdatePortfolioRequest request)
        {
            try
            {
                // Проверяем существование портфеля
                var exists = await _portfolioAppService.ExistsAsync(id);
                if (!exists)
                {
                    _logger.LogWarning("Попытка обновления несуществующего портфеля {PortfolioId}", id);
                    return NotFound();
                }

                var updatingPortfolioDto = new UpdatingPortfolioDto(request.Name, request.Currency, request.IsPrivate);
                await _portfolioAppService.UpdateAsync(id, updatingPortfolioDto);

                _logger.LogInformation("Портфель {PortfolioId} успешно обновлен", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при обновлении портфеля {PortfolioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Удалить портфель по Id
        /// </summary>
        /// <param name="id">Id портфеля</param>
        /// <returns></returns>
        [Authorize(Roles = "USER")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeletePortfolio(Guid id)
        {
            try
            {
                var result = await _portfolioAppService.DeleteAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден при попытке удаления", id);
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при удалении портфеля с ID {PortfolioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить расчет доходности портфеля с детализацией по активам
        /// </summary>
        /// <param name="id">Уникальный идентификатор портфеля</param>
        /// <param name="request">Параметры расчета доходности</param>
        /// <returns>
        /// 200 - Успешный возврат расчета доходности портфеля
        /// 404 - Портфель с указанным идентификатором не найден
        /// 500 - Внутренняя ошибка сервера при расчете доходности
        /// </returns>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/portfolios/{id}/profit-loss?calculationType=Current
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpGet("{id:guid}/profit-loss")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioProfitLossResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioProfitLossResponse>> GetPortfolioProfitLoss(
            Guid id,
            [FromQuery] CalculateProfitLossRequest request)
        {
            _logger.LogInformation("Запрос доходности портфеля ID: {PortfolioId}, тип расчета: {CalculationType}",
                id, request.CalculationType);

            try
            {
                var result = await _portfolioAppService.GetPortfolioProfitLossAsync(id, request.CalculationType);
                if (result == null)
                {
                    _logger.LogWarning("Портфель с ID {PortfolioId} не найден при запросе доходности", id);
                    return NotFound($"Портфель с ID {id} не найден");
                }

                _logger.LogInformation("Успешно возвращена доходность портфеля ID: {PortfolioId}", id);
                return Ok(result.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при расчете доходности портфеля ID: {PortfolioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Произошла ошибка при расчете доходности портфеля");
            }
        }

        /// <summary>
        /// Получить расчет доходности по всем активам портфеля
        /// </summary>
        /// <param name="id">Уникальный идентификатор портфеля</param>
        /// <param name="request">Параметры расчета доходности</param>
        /// <returns>
        /// 200 - Успешный возврат расчета доходности активов портфеля
        /// 404 - Портфель с указанным идентификатором не найден
        /// 500 - Внутренняя ошибка сервера при расчете доходности
        /// </returns>
        /// <remarks>
        /// Пример запроса:
        /// GET /api/v1/portfolios/{id}/assets/profit-loss?calculationType=Realized
        /// </remarks>
        [Authorize(Roles = "USER")]
        [HttpGet("{id:guid}/assets/profit-loss")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PortfolioAssetProfitLossItemResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PortfolioAssetProfitLossItemResponse>>> GetPortfolioAssetsProfitLoss(
            Guid id,
            [FromQuery] CalculateProfitLossRequest request)
        {
            _logger.LogInformation("Запрос доходности активов портфеля ID: {PortfolioId}", id);

            try
            {
                var result = await _portfolioAppService.GetPortfolioAssetsProfitLossAsync(id, request.CalculationType);

                _logger.LogInformation("Успешно возвращена доходность {AssetCount} активов портфеля ID: {PortfolioId}",
                    result.Count(), id);
                return Ok(result.Select(r => r.ToResponse()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении доходности активов портфеля ID: {PortfolioId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Произошла ошибка при расчете доходности активов портфеля");
            }
        }
    }
}
