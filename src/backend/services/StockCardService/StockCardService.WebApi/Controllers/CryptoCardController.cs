using Microsoft.AspNetCore.Mvc;
using StockCardService.WebApi.Models.CryptoCard;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Карточки криптовалют
    /// </summary>
    [Route("api/v1/[controller]")]
    public class CryptoCardController : ControllerBase
    {
        private readonly ICryptoCardService _cryptoCardService;

        public CryptoCardController(ICryptoCardService cryptoCardService)
        {
            _cryptoCardService = cryptoCardService;
        }

        /// <summary>
        /// Получить все карточки криптовалют
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CryptoCardModel>>> GetCryptoCardsAsync()
        {
            var cryptoCards = (await _cryptoCardService.GetAllAsync()).Select(CryptoCardMapper.ToModel).ToList();
            return cryptoCards;
        }

        /// <summary>
        /// Получить карточку криптовалюты по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CryptoCardModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<CryptoCardModel>> GetCryptoCardAsync(Guid id)
        {
            var cryptoCardDto = await _cryptoCardService.GetByIdAsync(id);

            if (cryptoCardDto == null)
                return NotFound();
            //var customersPreferencesList = (await _cryptoCardRepository.GetByIdWithPreferenceAsync(id)).CustomerPreferences.Select(cp => cp.Preference).ToList();

            var cryptoCardModel = CryptoCardMapper.ToModel(cryptoCardDto);
            return cryptoCardModel;
        }

        /// <summary>
        /// Добавить новую карточку криптовалюты
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(CryptoCardModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateCryptoCard(CreatingCryptoCardModel request)
        {
            var createdCryptoCardId = await _cryptoCardService.CreateAsync(CryptoCardMapper.ToDto(request));
            if (createdCryptoCardId == Guid.Empty) return Problem("Не удалось создать клиента");

            var cryptoCardModel = new CryptoCardModel()
            {
                Id = createdCryptoCardId,
                Ticker = request.Ticker,
                Name = request.Name,
                Description = request.Description
            };

            return CreatedAtRoute("GetCryptoCardModel", new { id = createdCryptoCardId }, cryptoCardModel);
        }

        /// <summary>
        /// Получить модель карточки криптовалюты
        /// </summary>
        /// <returns></returns>
        [HttpGet("Model/{id:guid}", Name = "GetCryptoCardModel")]
        [ProducesResponseType(typeof(CryptoCardModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CryptoCardModel>> GetCryptoCardModelAsync(Guid id)
        {
            var cryptoCardDto = await _cryptoCardService.GetByIdAsync(id);

            if (cryptoCardDto == null)
                return NotFound();

            var cryptoCardModel = CryptoCardMapper.ToModel(cryptoCardDto);
            return cryptoCardModel;
        }

        /// <summary>
        /// Обновить существующую карточку криптовалюты
        /// </summary>
        /// <param name="request"> Обновленная карточка криптовалюты. </param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditCryptoCardAsync(UpdatingCryptoCardModel request)
        {
            await _cryptoCardService.UpdateAsync(CryptoCardMapper.ToDto(request));
            return Ok();
        }

        /// <summary>
        /// Удалить карточку криптовалюты
        /// </summary>
        /// <param name="id"> Id карточки криптовалюты </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCryptoCard(Guid id)
        {
            await _cryptoCardService.DeleteAsync(id);
            return NoContent();
        }
    }
}
