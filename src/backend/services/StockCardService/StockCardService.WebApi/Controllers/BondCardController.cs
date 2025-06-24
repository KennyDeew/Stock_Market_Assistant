using Microsoft.AspNetCore.Mvc;
using StockCardService.Abstractions.Repositories;
using StockCardService.Domain.Entities;
using StockCardService.Infrastructure.Repositories;
using StockCardService.WebApi.Models.BondCard;
using StockCardService.WebApi.Models.ShareCard;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Карточки облигаций
    /// </summary>
    [Route("api/v1/[controller]")]
    public class BondCardController : ControllerBase
    {
        private readonly IRepository<BondCard, Guid> _bondCardRepository;

        public BondCardController(IRepository<BondCard, Guid> bondCardRepository)
        {
            _bondCardRepository = bondCardRepository;
        }

        /// <summary>
        /// Получить все карточки облигаций
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<BondCardModel>>> GetBondCardsAsync()
        {
            var bondCards = await _bondCardRepository.GetAllAsync(CancellationToken.None);

            var bondCardModelList = bondCards.Select(x =>
                new BondCardModel()
                {
                    Id = x.Id,
                    Ticker = x.Ticker,
                    Name = x.Name,
                    Description = x.Description,
                    Coupons = new List<Coupon>()
                }).ToList();

            return bondCardModelList;
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
            var bondCard = await _bondCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (bondCard == null)
                return NotFound();
            //var customersPreferencesList = (await _bondCardRepository.GetByIdWithPreferenceAsync(id)).CustomerPreferences.Select(cp => cp.Preference).ToList();

            var bondCardModel = new BondCardModel()
            {
                Id = bondCard.Id,
                Ticker = bondCard.Ticker,
                Name = bondCard.Name,
                Description = bondCard.Description
            };
            return bondCardModel;
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
            var bondCard = new BondCard()
            {
                Id = Guid.NewGuid(),
                Ticker = request.Ticker,
                Name = request.Name,
                Description = request.Description
            };
            
            var createdBondCard = await _bondCardRepository.AddAsync(bondCard);
            if (createdBondCard == null) return Problem("Не удалось создать клиента");
            var bondCardShortModel = new BondCardShortModel()
            {
                Id = createdBondCard.Id,
                Ticker = createdBondCard.Ticker,
                Name = createdBondCard.Name,
                Description = createdBondCard.Description
            };

            return CreatedAtRoute("GetBondCardShortModel", new { id = createdBondCard.Id }, bondCardShortModel);
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
            var customer = await _bondCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (customer == null)
                return NotFound();

            var bondCardShortModel = new BondCardShortModel()
            {
                Id = customer.Id,
                Ticker = customer.Ticker,
                Name = customer.Name,
                Description = customer.Description
            };

            return Ok(bondCardShortModel);
        }

        /// <summary>
        /// Обновить существующую карточку облигации
        /// </summary>
        /// <param name="request"> Обновленная карточка облигации. </param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> EditBondCardAsync(UpdatingShareCardModel request)
        {
            var shareCard = await _bondCardRepository.GetByIdAsync(request.Id, CancellationToken.None);
            if (shareCard == null)
                return NotFound();
            shareCard.Id = request.Id;
            shareCard.Ticker = request.Ticker;
            shareCard.Name = request.Name;
            shareCard.Description = request.Description;
            await _bondCardRepository.UpdateAsync(shareCard);
            return Ok();
        }

        /// <summary>
        /// Удалить карточку облигации
        /// </summary>
        /// <param name="id"> Id карточки облигации </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteBondCard(Guid id)
        {
            await _bondCardRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
