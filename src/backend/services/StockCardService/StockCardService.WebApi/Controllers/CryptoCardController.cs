using Microsoft.AspNetCore.Mvc;
using StockCardService.Abstractions.Repositories;
using StockCardService.WebApi.Models.CryptoCard;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockCardService.WebApi.Controllers
{
    /// <summary>
    /// Карточки криптовалют
    /// </summary>
    [Route("api/v1/[controller]")]
    public class CryptoCardController : ControllerBase
    {
        private readonly IRepository<CryptoCard, Guid> _cryptoCardRepository;

        public CryptoCardController(IRepository<CryptoCard, Guid> cryptoCardRepository)
        {
            _cryptoCardRepository = cryptoCardRepository;
        }

        /// <summary>
        /// Получить все карточки криптовалют
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CryptoCardModel>>> GetCryptoCardsAsync()
        {
            var cryptoCards = await _cryptoCardRepository.GetAllAsync(CancellationToken.None);

            var cryptoCardModelList = cryptoCards.Select(x =>
                new CryptoCardModel()
                {
                    Id = x.Id,
                    Ticker = x.Ticker,
                    Name = x.Name,
                    Description = x.Description
                }).ToList();

            return cryptoCardModelList;
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
        public async Task<ActionResult<CryptoCardModel>> GetCustomerAsync(Guid id)
        {
            var cryptoCard = await _cryptoCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (cryptoCard == null)
                return NotFound();
            //var customersPreferencesList = (await _cryptoCardRepository.GetByIdWithPreferenceAsync(id)).CustomerPreferences.Select(cp => cp.Preference).ToList();

            var cryptoCardModel = new CryptoCardModel()
            {
                Id = cryptoCard.Id,
                Ticker = cryptoCard.Ticker,
                Name = cryptoCard.Name,
                Description = cryptoCard.Description
            };
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
            var cryptoCard = new CryptoCard()
            {
                Id = Guid.NewGuid(),
                Ticker = request.Ticker,
                Name = request.Name,
                Description = request.Description
            };
            
            var createdCryptoCard = await _cryptoCardRepository.AddAsync(cryptoCard);
            if (createdCryptoCard == null) return Problem("Не удалось создать клиента");
            var cryptoCardModel = new CryptoCardModel()
            {
                Id = createdCryptoCard.Id,
                Ticker = createdCryptoCard.Ticker,
                Name = createdCryptoCard.Name,
                Description = createdCryptoCard.Description
            };

            return CreatedAtRoute("GetCryptoCardModel", new { id = createdCryptoCard.Id }, cryptoCardModel);
        }

        /// <summary>
        /// Получить модель карточки криптовалюты
        /// </summary>
        /// <returns></returns>
        [HttpGet("Model/{id:guid}", Name = "GetCryptoCardModel")]
        [ProducesResponseType(typeof(CryptoCardModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CryptoCardModel>> GetShortCustomerByIdAsync(Guid id)
        {
            var customer = await _cryptoCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (customer == null)
                return NotFound();

            var cryptoCardModel = new CryptoCardModel()
            {
                Id = customer.Id,
                Ticker = customer.Ticker,
                Name = customer.Name,
                Description = customer.Description
            };

            return Ok(cryptoCardModel);
        }
    }
}
