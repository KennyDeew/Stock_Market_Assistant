using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.WebApi.Models;

namespace StockMarketAssistant.PortfolioService.WebApi.Controllers
{
    /// <summary>
    /// Портфели
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [OpenApiTag("Portfolios")]
    public class PortfoliosController(IPortfolioAppService service, ILogger<PortfoliosController> logger) : ControllerBase
    {
        private readonly IPortfolioAppService _service = service;
        private readonly ILogger<PortfoliosController> _logger = logger;

        /// <summary>
        /// Получить список всех портфелей от всех пользователей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PortfolioShortResponse>))]
        public async Task<ActionResult<IEnumerable<PortfolioShortResponse>>> GetPortfoliosAsync()
        {
            var portfoliosDtos = await _service.GetAllAsync();

            var portfoliosModels = portfoliosDtos.Select(p =>
                new PortfolioShortResponse(p.Id, p.UserId, p.Name, p.Currency));

            return Ok(portfoliosModels);
        }

        /// <summary>
        /// Создать новый портфель
        /// </summary>
        /// <param name="request">Параметры создаваемого портфеля</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PortfolioShortResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<PortfolioShortResponse>> CreatePortfolioAsync(CreatePortfolioRequest request)
        {
            CreatingPortfolioDto creatingPortfolioDto = new(request.UserId, request.Name, request.Currency);
            var createdPortfolioId = await _service.CreateAsync(creatingPortfolioDto);
            if (createdPortfolioId == Guid.Empty)
                return BadRequest("Произошла ошибка при создании портфеля");

            var createdPortfolioShortResponse = new PortfolioShortResponse(createdPortfolioId, creatingPortfolioDto.UserId, creatingPortfolioDto.Name, creatingPortfolioDto.Currency);
            return Created(string.Empty, createdPortfolioShortResponse);
        }

        /// <summary>
        /// Получить данные портфеля по Id
        /// </summary>
        /// <param name="id">Id портфеля</param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortfolioResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<ActionResult<PortfolioResponse>> GetPortfolioAsync(Guid id)
        {
            PortfolioDto? portfolioDto = await _service.GetByIdAsync(id);
            if (portfolioDto is null)
                return NotFound($"Портфель с id {id} не найден");

            var portfolioModel = new PortfolioResponse()
            {
                Id = portfolioDto.Id,
                UserId = portfolioDto.UserId,
                Name = portfolioDto.Name,
                Currency = portfolioDto.Currency,
                Assets = portfolioDto.Assets.Select(sa => new PortfolioAssetShortResponse(sa.Id, sa.Ticker, sa.Quantity, sa.AveragePurchasePrice))
            };
            return Ok(portfolioModel);
        }

        /// <summary>
        /// Редактировать портфель
        /// </summary>
        /// <param name="id">Id портфеля</param>
        /// <param name="request">Данные для редактирования портфеля</param>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        public async Task<IActionResult> UpdatePortfolioAsync(Guid id, [FromBody] UpdatePortfolioRequest request)
        {
            if (request is null)
                return BadRequest("Некорректные данные для обновления характеристик портфеля");

            if (!await _service.ExistsAsync(id))
                return NotFound($"Портфель с id {id} не найден");

            var updatingPortfolioDto = new UpdatingPortfolioDto(request.Name, request.Currency);
            await _service.UpdateAsync(id, updatingPortfolioDto);
            return NoContent();
        }

        /// <summary>
        /// Удалить портфель по Id
        /// </summary>
        /// <param name="id">Id портфеля</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePortfolioAsync(Guid id)
        {
            if (!await _service.ExistsAsync(id))
                return NotFound($"Портфель с id {id} не найден");

            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
