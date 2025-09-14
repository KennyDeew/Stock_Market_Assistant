using Microsoft.AspNetCore.Mvc;
using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Карточки акций
    /// </summary>
    [Route("api/v1/[controller]")]
    public class ShareCardController : ControllerBase
    {
        private readonly IShareCardService _shareCardService;

        /// <summary>
        /// Конструктор контроллера карточки акции
        /// </summary>
        /// <param name="shareCardService"></param>
        public ShareCardController(IShareCardService shareCardService)
        {
            _shareCardService = shareCardService;
        }

        /// <summary>
        /// Получить все карточки акции
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<ShareCardModel>>> GetShareCardsAsync()
        {
            var shareCards = (await _shareCardService.GetAllAsync()).Select(ShareCardMapper.ToModel).ToList();
            return shareCards;
        }

        /// <summary>
        /// Получить карточку акции по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShareCardModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        public async Task<ActionResult<ShareCardModel>> GetShareCardAsync(Guid id)
        {
            var shareCard = await _shareCardService.GetByIdWithLinkedItemsAsync(id);

            if (shareCard == null)
                return NotFound();
            
            var shareCardModel = ShareCardMapper.ToModel(shareCard);
            return shareCardModel;
        }

        /// <summary>
        /// Получить неполные данные карточки акции
        /// </summary>
        /// <returns></returns>
        [HttpGet("short/{id:guid}", Name = "GetShareCardShortModel")]
        [ProducesResponseType(typeof(ShareCardShortModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ShareCardShortModel>> GetShortShareCardModelAsync(Guid id)
        {
            var shareCard = await _shareCardService.GetShortByIdAsync(id);

            if (shareCard == null)
                return NotFound();

            var shareCardShortModel = ShareCardMapper.ToModel(shareCard);

            return Ok(shareCardShortModel);
        }

        /// <summary>
        /// Добавить новую карточку акции
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ShareCardModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateShareCard(CreatingShareCardModel request)
        {
            var newShareCardId = await _shareCardService.CreateAsync(ShareCardMapper.ToDto(request));
            if (newShareCardId == Guid.Empty) return Problem("Не удалось создать карточку акции");
            //Проверяем созданную карточку
            var shareCardShortModelForChecking = new ShareCardShortModel()
            {
                Id = newShareCardId,
                Ticker = request.Ticker,
                Name = request.Name,
                Description = request.Description,
                Currency = request.Currency,
            };

            return CreatedAtRoute("GetShareCardShortModel", new { id = newShareCardId }, shareCardShortModelForChecking);
        }

        /// <summary>
        /// Обновить существующую карточку акции
        /// </summary>
        /// <param name="request"> Обновленная карточка акции. </param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditShareCardAsync(UpdatingShareCardModel request)
        {
            await _shareCardService.UpdateAsync(ShareCardMapper.ToDto(request));
            return Ok();
        }

        /// <summary>
        /// Обновить все цены акций
        /// </summary>
        /// <returns></returns>
        [HttpPut("UpdateAllPrices", Name = "UpdateShareCardPrices")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAllShareCardPriceAsync()
        {
            await _shareCardService.UpdateShareCardPricesAsync();
            return Ok();
        }

        /// <summary>
        /// Удалить карточку акции
        /// </summary>
        /// <param name="id"> Id карточки акции </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteShareCard(Guid id)
        {
            await _shareCardService.DeleteAsync(id);
            return NoContent();
        }
    }
}
