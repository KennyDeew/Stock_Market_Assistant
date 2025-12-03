using Microsoft.AspNetCore.Mvc;
using StockCardService.WebApi.Models.BondCard;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Карточки облигаций
    /// </summary>
    [Route("api/v1/[controller]")]
    public class BondCardController : ControllerBase
    {
        private readonly IBondCardService _bondCardService;

        /// <summary>
        /// Конструктор контроллера карточки облигации
        /// </summary>
        /// <param name="bondCardService"></param>
        public BondCardController(IBondCardService bondCardService)
        {
            _bondCardService = bondCardService;
        }

        /// <summary>
        /// Получить все карточки облигаций
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<BondCardModel>>> GetBondCardsAsync()
        {
            var bondCards = (await _bondCardService.GetAllAsync()).Select(BondCardMapper.ToModel).ToList();
            return bondCards;
        }

        /// <summary>
        /// Получить карточку облигации по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BondCardModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<BondCardModel>> GetBondCardAsync(Guid id)
        {
            var bondCardDto = await _bondCardService.GetByIdWithLinkedItemsAsync(id);

            if (bondCardDto == null)
                return NotFound();
            
            var bondCardModel = BondCardMapper.ToModel(bondCardDto);
            return bondCardModel;
        }

        /// <summary>
        /// Получить неполные данные карточки облигации
        /// </summary>
        /// <returns></returns>
        [HttpGet("short/{id:guid}", Name = "GetBondCardShortModel")]
        [ProducesResponseType(typeof(BondCardShortModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<BondCardShortModel>> GetShortBondCardModelAsync(Guid id)
        {
            var bondCardShortDto = await _bondCardService.GetShortByIdAsync(id);

            if (bondCardShortDto == null)
                return NotFound();

            var bondCardShortModel = BondCardMapper.ToModel(bondCardShortDto); ;

            return Ok(bondCardShortModel);
        }

        /// <summary>
        /// Добавить новую карточку облигации
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BondCardModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateBondCard(CreatingBondCardModel request)
        {
            var newBondCardId = await _bondCardService.CreateAsync(BondCardMapper.ToDto(request));
            if (newBondCardId == Guid.Empty) return Problem("Не удалось создать клиента");

            var bondCardShortModel = new BondCardShortModel()
            {
                Id = newBondCardId,
                Ticker = request.Ticker,
                Name = request.Name,
                Board = request.Board,
                Description = request.Description,
                Currency = request.Currency,
                FaceValue = request.FaceValue,
                Rating = request.Rating,
                MaturityPeriod = request.MaturityPeriod.ToString()
            };

            return CreatedAtRoute("GetBondCardShortModel", new { id = newBondCardId }, bondCardShortModel);
        }

        /// <summary>
        /// Обновить существующую карточку облигации
        /// </summary>
        /// <param name="request">Обновленная карточка облигации.</param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditBondCardAsync(UpdatingBondCardModel request)
        {
            var cre = BondCardMapper.ToDto(request);
            await _bondCardService.UpdateAsync(cre);
            return Ok();
        }

        /// <summary>
        /// Обновить все цены облигаций
        /// </summary>
        /// <returns></returns>
        [HttpPut("UpdateAllPrices", Name = "UpdateBondCardPrices")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAllShareCardPriceAsync()
        {
            await _bondCardService.UpdateBondCardPricesAsync();
            return Ok();
        }

        /// <summary>
        /// Удалить карточку облигации
        /// </summary>
        /// <param name="id">Id карточки облигации</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBondCard(Guid id)
        {
            await _bondCardService.DeleteAsync(id);
            return NoContent();
        }
    }
}
