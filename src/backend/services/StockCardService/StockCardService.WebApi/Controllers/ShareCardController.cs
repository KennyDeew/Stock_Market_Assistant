using Microsoft.AspNetCore.Mvc;
using StockCardService.Abstractions.Repositories;
using StockCardService.Domain.Entities;
using StockCardService.WebApi.Models._01sub_Dividend;
using StockCardService.WebApi.Models._01sub_FinancialReport;
using StockCardService.WebApi.Models._01sub_Multiplier;
using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Карточки акций
    /// </summary>
    [Route("api/v1/[controller]")]
    public class ShareCardController : ControllerBase
    {
        private readonly IRepository<ShareCard, Guid> _shareCardRepository;

        public ShareCardController(IRepository<ShareCard, Guid> shareCardRepository)
        {
            _shareCardRepository = shareCardRepository;
        }

        /// <summary>
        /// Получить все карточки акции
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<ShareCardModel>>> GetShareCardsAsync()
        {
            var shareCards = await _shareCardRepository.GetAllAsync(CancellationToken.None);

            var shareCardModelList = shareCards.Select(x =>
                new ShareCardModel()
                {
                    Id = x.Id,
                    Ticker = x.Ticker,
                    Name = x.Name,
                    Description = x.Description,
                    FinancialReports = new List<FinancialReportModel>(),
                    Multipliers = new List<MultiplierModel>(),
                    Dividends = new List<DividendModel>()
                }).ToList();

            return shareCardModelList;
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
            var shareCard = await _shareCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (shareCard == null)
                return NotFound();
            //var customersPreferencesList = (await _shareCardRepository.GetByIdWithPreferenceAsync(id)).CustomerPreferences.Select(cp => cp.Preference).ToList();

            var shareCardModel = new ShareCardModel()
            {
                Id = shareCard.Id,
                Ticker = shareCard.Ticker,
                Name = shareCard.Name,
                Description = shareCard.Description
            };
            return shareCardModel;
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
            var shareCard = new ShareCard()
            {
                Id = Guid.NewGuid(),
                Ticker = request.Ticker,
                Name = request.Name,
                Description = request.Description
            };
            
            var createdShareCard = await _shareCardRepository.AddAsync(shareCard);
            if (createdShareCard == null) return Problem("Не удалось создать клиента");
            var shareCardShortModel = new ShareCardShortModel()
            {
                Id = createdShareCard.Id,
                Ticker = createdShareCard.Ticker,
                Name = createdShareCard.Name,
                Description = createdShareCard.Description
            };

            return CreatedAtRoute("GetShareCardShortModel", new { id = createdShareCard.Id }, shareCardShortModel);
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
            var customer = await _shareCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (customer == null)
                return NotFound();

            var shareCardShortModel = new ShareCardShortModel()
            {
                Id = customer.Id,
                Ticker = customer.Ticker,
                Name = customer.Name,
                Description = customer.Description
            };

            return Ok(shareCardShortModel);
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
            var shareCard = await _shareCardRepository.GetByIdAsync(request.Id, CancellationToken.None);
            if (shareCard == null)
                return NotFound();
            shareCard.Id = request.Id;
            shareCard.Ticker = request.Ticker;
            shareCard.Name = request.Name;
            shareCard.Description = request.Description;
            await _shareCardRepository.UpdateAsync(shareCard);
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
            await _shareCardRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
