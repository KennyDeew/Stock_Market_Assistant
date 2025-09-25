using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Controllers
{
    /// <summary>
    /// Активы ценных бумаг портфеля
    /// </summary>
    [ApiController]
    //[Route("api/v1/[controller]")]
    [Route("api/v1/portfolio-assets")]
    [Produces("application/json")]
    [OpenApiTag("PortfolioAssets")]
    public class PortfolioAssetsController(IPortfolioAppService portfolioAppService, IPortfolioAssetAppService portfolioAssetAppService, ILogger<PortfolioAssetsController> logger) : ControllerBase
    {
        private readonly IPortfolioAppService _portfolioAppService = portfolioAppService;
        private readonly IPortfolioAssetAppService _portfolioAssetAppService = portfolioAssetAppService;
        private readonly ILogger<PortfolioAssetsController> _logger = logger;

        /// <summary>
        /// Создать новый финансовый актив в портфеле с начальной транзакцией покупки
        /// </summary>
        /// <param name="request">Параметры создаваемого актива ценной бумаги</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PortfolioAssetShortResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<PortfolioAssetShortResponse>> CreatePortfolioAsset([FromBody]CreatePortfolioAssetRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (!await _portfolioAppService.ExistsAsync(request.PortfolioId))
                {
                    _logger.LogWarning("Попытка создания актива для несуществующего портфеля {PortfolioId}", request.PortfolioId);
                    return NotFound();
                }

                CreatingPortfolioAssetDto createDto = new(request.PortfolioId, request.StockCardId, request.AssetType, request.PurchasePricePerUnit, request.Quantity);
                PortfolioAssetDto result = await _portfolioAssetAppService.CreateAsync(createDto);
                PortfolioAssetShortResponse response = new(result.Id, result.PortfolioId, result.Ticker ?? string.Empty, result.TotalQuantity, result.AveragePurchasePrice);
                return CreatedAtAction(nameof(GetPortfolioAssetById), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при создании актива портфеля для портфеля {PortfolioId}", request.PortfolioId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить данные актива ценной бумаги из портфеля по Id
        /// </summary>
        /// <param name="id">Id актива ценной бумаги из портфеля</param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioAssetResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioAssetResponse>> GetPortfolioAssetById(Guid id)
        {
            try
            {
                PortfolioAssetDto? result = await _portfolioAssetAppService.GetByIdAsync(id);
                if (result is null)
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден", id);
                    return NotFound();
                }

                var response = new PortfolioAssetResponse
                (result.Id, result.PortfolioId, result.StockCardId,
                result.Ticker, result.Name, result.Description, result.TotalQuantity,
                result.AveragePurchasePrice, result.LastUpdated, result.Currency)
                {
                    AssetType = result.AssetType,
                    Transactions = [.. result.Transactions.Select(t =>
                        new PortfolioAssetTransactionResponse(t.Id, t.PortfolioAssetId, t.Quantity, t.PricePerUnit, t.Currency, t.TransactionDate)
                        {
                            TransactionType = t.TransactionType
                        })]
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении актива портфеля с ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить актив портфеля по Id
        /// </summary>
        /// <param name="id">Id актива ценной бумаги из портфеля</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePortfolioAsset(Guid id)
        {
            try
            {
                if (!await _portfolioAssetAppService.ExistsAsync(id))
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден при попытке удаления", id);
                    return NotFound();
                }
                await _portfolioAssetAppService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении актива портфеля с ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить данные по транзакции актива портфеля по Id
        /// </summary>
        /// <param name="transactionId">Id транзакции</param>
        /// <param name="assetId">Id актива</param>
        /// <returns></returns>
        [HttpGet("{assetId:guid}/transactions/{transactionId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioAssetTransactionResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PortfolioAssetTransactionResponse>> GetPortfolioAssetTransactionById(
            Guid assetId,
            Guid transactionId)
        {
            try
            {
                PortfolioAssetTransactionDto? result = await _portfolioAssetAppService.GetAssetTransactionByIdAsync(transactionId);
                if (result is null || result.PortfolioAssetId != assetId)
                {
                    _logger.LogWarning("Транзакция по активу с ID {TransactionId} не найдена", transactionId);
                    return NotFound();
                }

                var response = new PortfolioAssetTransactionResponse(
                    result.Id,
                    result.PortfolioAssetId,
                    result.Quantity,
                    result.PricePerUnit,
                    result.Currency,
                    result.TransactionDate)
                {
                    TransactionType = result.TransactionType
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении транзакции {TransactionId} актива {AssetId}",
                                transactionId, assetId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить транзакции актива портфеля за указанный период
        /// </summary>
        /// <param name="assetId">Идентификатор актива портфеля</param>
        /// <param name="startDate">Начальная дата периода (включительно)</param>
        /// <param name="endDate">Конечная дата периода (включительно)</param>
        /// <returns>Список транзакций за период</returns>
        [HttpGet("{assetId:guid}/transactions/period")]
        [ProducesResponseType(typeof(IEnumerable<PortfolioAssetTransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PortfolioAssetTransactionResponse>>> GetPortfolioAssetTransactionsByPeriod(
            Guid assetId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                // Валидация дат
                if (startDate > endDate)
                {
                    return BadRequest("Начальная дата не может быть позже конечной даты");
                }

                // Проверяем существование актива
                if (!await _portfolioAssetAppService.ExistsAsync(assetId))
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден", assetId);
                    return NotFound();
                }

                // Получаем транзакции за период
                var transactions = await _portfolioAssetAppService.GetAssetTransactionsByAssetIdAndPeriodAsync(
                    assetId, startDate, endDate);

                // Преобразуем в response модели
                var response = transactions.Select(t => new PortfolioAssetTransactionResponse(
                    t.Id,
                    t.PortfolioAssetId,
                    t.Quantity,
                    t.PricePerUnit,
                    t.Currency,
                    t.TransactionDate)
                {
                    TransactionType = t.TransactionType
                }).ToArray();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении транзакций актива {AssetId} за период {StartDate} - {EndDate}",
                                assetId, startDate, endDate);
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Добавить транзакцию к активу портфеля
        /// </summary>
        /// <param name="assetId">Идентификатор актива</param>
        /// <param name="request">Параметры добавляемой транзакции к активу</param>
        [HttpPost("{assetId:guid}/transactions")]
        [ProducesResponseType(typeof(PortfolioAssetTransactionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PortfolioAssetTransactionResponse>> AddPortfolioAssetTransaction(
            Guid assetId,
            [FromBody] CreatePortfolioAssetTransactionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (!await _portfolioAssetAppService.ExistsAsync(assetId))
                {
                    _logger.LogWarning("Актив портфеля с ID {AssetId} не найден", assetId);
                    return NotFound();
                }

                CreatingPortfolioAssetTransactionDto createDto = new(request.TransactionType, request.Quantity, request.PricePerUnit, request.TransactionDate);
                PortfolioAssetTransactionDto result = await _portfolioAssetAppService.AddAssetTransactionAsync(assetId, createDto);

                var response = new PortfolioAssetTransactionResponse(
                    result.Id,
                    result.PortfolioAssetId,
                    result.Quantity,
                    result.PricePerUnit,
                    result.Currency,
                    result.TransactionDate)
                {
                    TransactionType = result.TransactionType
                };

                return CreatedAtAction(
                    nameof(GetPortfolioAssetTransactionById),
                    new { assetId = result.PortfolioAssetId, transactionId = response.Id },
                    response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении транзакции к активу портфеля");
                return BadRequest("Не удалось добавить транзакцию");
            }
        }

        /// <summary>
        /// Обновить транзакцию актива портфеля
        /// </summary>
        /// <param name="transactionId">Идентификатор транзакции</param>
        /// <param name="request">Данные для редактирования транзакции актива портфеля</param>
        /// <param name="assetId">Идентификатор актива</param>
        [HttpPut("{assetId:guid}/transactions/{transactionId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<PortfolioAssetTransactionResponse>> UpdatePortfolioAssetTransaction(
            Guid assetId,
            Guid transactionId,
            [FromBody] UpdatePortfolioAssetTransactionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var transaction = await _portfolioAssetAppService.GetAssetTransactionByIdAsync(transactionId);
                if (transaction is null || transaction.PortfolioAssetId != assetId)
                {
                    _logger.LogWarning("Транзакция актива портфеля с ID {TransactionId} не найден", transactionId);
                    return NotFound();
                }

                var updateDto = new UpdatingPortfolioAssetTransactionDto(
                    request.TransactionType,
                    request.Quantity,
                    request.PricePerUnit,
                    request.TransactionDate,
                    request.Currency);

                await _portfolioAssetAppService.UpdateAssetTransactionAsync(transactionId, updateDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении транзакции с ID: {TransactionId}", transactionId);
                return BadRequest("Не удалось обновить транзакцию");
            }
        }

        /// <summary>
        /// Удалить транзакцию актива портфеля по Id
        /// </summary>
        /// <param name="assetId">Идентификатор актива</param>
        /// <param name="transactionId">Идентификатор транзакции</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{assetId:guid}/transactions/{transactionId:guid}")]
        public async Task<IActionResult> DeletePortfolioAssetTransaction(Guid assetId, Guid transactionId)
        {
            try
            {
                var transaction = await _portfolioAssetAppService.GetAssetTransactionByIdAsync(transactionId);

                // Проверяем существование транзакции и принадлежность активу
                if (transaction is null || transaction.PortfolioAssetId != assetId)
                {
                    _logger.LogWarning("Транзакция {TransactionId} для актива {AssetId} не найдена при попытке удаления",
                                     transactionId, assetId);
                    return NotFound();
                }

                var result = await _portfolioAssetAppService.DeleteAssetTransactionAsync(transactionId);

                if (!result)
                {
                    _logger.LogWarning("Не удалось удалить транзакцию {TransactionId} актива {AssetId}", transactionId, assetId);
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при удалении транзакции {TransactionId} актива {AssetId}",
                                transactionId, assetId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                 new { error = "InternalError", message = "Внутренняя ошибка сервера" });
            }
        }
    }
}
