using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Controllers
{
    /// <summary>
    /// Активы ценных бумаг портфеля
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [OpenApiTag("PortfolioAssets")]
    public class PortfolioAssetsController(IPortfolioAppService portfolioService, IPortfolioAssetAppService portfolioAssetService, ILogger<PortfolioAssetsController> logger) : ControllerBase
    {
        private readonly IPortfolioAppService _portfolioService = portfolioService;
        private readonly IPortfolioAssetAppService _portfolioAssetService = portfolioAssetService;
        private readonly ILogger<PortfolioAssetsController> _logger = logger;

        /// <summary>
        /// Создать новый актив ценной бумаги в портфеле
        /// </summary>
        /// <param name="request">Параметры создаваемого актива ценной бумаги</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PortfolioAssetShortResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<PortfolioAssetShortResponse>> CreatePortfolioAssetAsync(CreatePortfolioAssetRequest request)
        {
            if (request is null)
                return BadRequest("Некорректные данные для создания актива ценной бумаги в портфеле");

            if (!await _portfolioService.ExistsAsync(request.PortfolioId))
                return NotFound($"Портфель с id {request.PortfolioId} не существует");

            CreatingPortfolioAssetDto creatingPortfolioAssetDto = new(request.PortfolioId, request.StockCardId, request.AssetType);
            var createdPortfolioAssetId = await _portfolioAssetService.CreateAsync(creatingPortfolioAssetDto);
            if (createdPortfolioAssetId == Guid.Empty)
                return BadRequest("Произошла ошибка при создании актива ценной бумаги в портфеле");

            PortfolioAssetDto? portfolioAssetDto = await _portfolioAssetService.GetByIdAsync(createdPortfolioAssetId);
            var createdPortfolioAssetShortResponse = new PortfolioAssetShortResponse(createdPortfolioAssetId, portfolioAssetDto?.Ticker ?? string.Empty);
            return Created(string.Empty, createdPortfolioAssetShortResponse);
        }

        /// <summary>
        /// Получить данные актива ценной бумаги из портфеля по Id
        /// </summary>
        /// <param name="id">Id актива ценной бумаги из портфеля</param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioAssetResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<PortfolioAssetResponse>> GetPortfolioAssetAsync(Guid id)
        {
            PortfolioAssetDto? portfolioAssetDto = await _portfolioAssetService.GetByIdAsync(id);
            if (portfolioAssetDto is null)
                return NotFound($"Актив ценной бумаги с id {id} не найден");
            
            var portfolioAssetModel = new PortfolioAssetResponse
            (portfolioAssetDto.Id, portfolioAssetDto.PortfolioId, portfolioAssetDto.StockCardId, portfolioAssetDto.AssetType.GetEnumDescription(),
            portfolioAssetDto.Ticker, portfolioAssetDto.Name, portfolioAssetDto.Description, portfolioAssetDto.Quantity,
            portfolioAssetDto.AveragePurchasePrice, portfolioAssetDto.LastUpdated, portfolioAssetDto.Currency);
            return Ok(portfolioAssetModel);
        }

        /// <summary>
        /// Редактировать актив ценной бумаги в портфеле
        /// </summary>
        /// <param name="id">Id актива ценной бумаги из портфеля</param>
        /// <param name="request">Данные для редактирования портфеля</param>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdatePortfolioAssetAsync(Guid id, [FromBody] UpdatePortfolioAssetRequest request)
        {
            if (request is null)
                return BadRequest("Некорректные данные для обновления характеристик актива ценной бумаги в портфеле");

            if (!await _portfolioAssetService.ExistsAsync(id))
                return NotFound($"Актив ценной бумаги с id {id} не найден");

            var updatingPortfolioAssetDto = new UpdatingPortfolioAssetDto(request.Quantity, request.AveragePurchasePrice, request.LastUpdated, request.Currency);
            await _portfolioAssetService.UpdateAsync(id, updatingPortfolioAssetDto);
            return NoContent();
        }

        /// <summary>
        /// Удалить актив портфеля по Id
        /// </summary>
        /// <param name="id">Id актива ценной бумаги из портфеля</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePortfolioAssetAsync(Guid id)
        {
            if (!await _portfolioAssetService.ExistsAsync(id))
                return NotFound($"Актив ценной бумаги с id {id} не найден");

            await _portfolioAssetService.DeleteAsync(id);
            return NoContent();
        }
    }
}
