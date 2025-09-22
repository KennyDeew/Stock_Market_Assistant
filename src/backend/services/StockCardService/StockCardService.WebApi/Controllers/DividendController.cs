using Microsoft.AspNetCore.Mvc;
using StockCardService.WebApi.Models._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Дивиденды
    /// </summary>
    [Route("api/v1/[controller]")]
    public class DividendController : ControllerBase
    {
        private readonly IDividendService _dividendService;

        /// <summary>
        /// Конструктор контроллера дивиденда акции
        /// </summary>
        /// <param name="dividendService"></param>
        public DividendController(IDividendService dividendService)
        {
            _dividendService = dividendService;
        }

        /// <summary>
        /// Получить все дивиденды всех акций
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<DividendModel>>> GetAllDividendsAsync()
        {
            var dividends = (await _dividendService.GetAllAsync()).Select(DividendMapper.ToModel).ToList();
            return dividends;
        }

        /// <summary>
        /// Получить все дивиденды определенной акции
        /// </summary>
        /// <returns></returns>
        [HttpGet("ByShareCard/{shareCardId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DividendModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<List<DividendModel>>> GetAllDividendsByParentIdAsync(Guid shareCardId)
        {
            var dividends = (await _dividendService.GetAllByShareCardIdAsync(shareCardId)).Select(DividendMapper.ToModel).ToList();
            return dividends;
        }

        /// <summary>
        /// Получить дивиденд по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DividendModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<DividendModel>> GetDividendByIdAsync(Guid id)
        {
            var dividendDto = await _dividendService.GetByIdAsync(id);

            if (dividendDto == null)
                return NotFound();

            var dividendModel = DividendMapper.ToModel(dividendDto);
            return dividendModel;
        }

        /// <summary>
        /// Добавить новый дивиденд
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(DividendModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDividend(CreatingDividendModel request)
        {
            var createdDividendId = await _dividendService.CreateAsync(DividendMapper.ToDto(request));
            if (createdDividendId == Guid.Empty) return Problem("Не удалось создать клиента");

            var dividendModel = new DividendModel()
            {
                Id = createdDividendId,
                ShareCardId = request.ShareCardId,
                Currency = request.Currency,
                CutOffDate = request.CutOffDate,
                Period = request.Period,
                Value = request.Value
            };

            return CreatedAtRoute("GetDividendModel", new { id = createdDividendId }, dividendModel);
        }

        /// <summary>
        /// Получить модель дивиденда
        /// </summary>
        /// <returns></returns>
        [HttpGet("Model/{id:guid}", Name = "GetDividendModel")]
        [ProducesResponseType(typeof(DividendModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DividendModel>> GetDividendModelAsync(Guid id)
        {
            var dividendDto = await _dividendService.GetByIdAsync(id);

            if (dividendDto == null)
                return NotFound();

            var dividendModel = DividendMapper.ToModel(dividendDto);
            return dividendModel;
        }

        /// <summary>
        /// Обновить информацию о дивиденде
        /// </summary>
        /// <param name="request"> Обновленная карточка дивиденда. </param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditDividendAsync(UpdatingDividendModel request)
        {
            await _dividendService.UpdateAsync(DividendMapper.ToDto(request));
            return Ok();
        }

        /// <summary>
        /// Удалить дивиденд
        /// </summary>
        /// <param name="id"> Id дивиденда</param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDividend(Guid id)
        {
            await _dividendService.DeleteAsync(id);
            return NoContent();
        }
    }
}
