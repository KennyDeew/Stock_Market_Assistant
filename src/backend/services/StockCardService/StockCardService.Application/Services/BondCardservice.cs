using Microsoft.Extensions.Logging;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard;
using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;
using System.Collections.Concurrent;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    /// <summary>
    /// Сервис работы с карточкой облигации
    /// </summary>
    public class BondCardservice : IBondCardService
    {
        /// <summary>
        /// Репозиторий облигаций
        /// </summary>
        private readonly IRepository<BondCard, Guid> _bondCardRepository;

        /// <summary>
        /// Сервис для обновления цены облигации
        /// </summary>
        private readonly IStockPriceService _stockPriceService;

        /// <summary>
        /// Логгер для регистрации событий и ошибок.
        /// </summary>
        private readonly ILogger<BondCardservice> _logger;

        public BondCardservice(IRepository<BondCard, Guid> bondCardRepository, IStockPriceService stockPriceService, ILogger<BondCardservice> logger)
        {
            _bondCardRepository = bondCardRepository;
            _stockPriceService = stockPriceService;
            _logger = logger;
        }

        /// <summary>
        /// Получить все карточки облигаций
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<BondCardDto>> GetAllAsync()
        {
            var bondCards = await _bondCardRepository.GetAllAsync(CancellationToken.None);

            var bondCardDtoList = bondCards.Select(x =>
                new BondCardDto()
                {
                    Id = x.Id,
                    Ticker = x.Ticker,
                    Name = x.Name,
                    Board = x.Board,
                    Description = x.Description,
                    MaturityPeriod = x.MaturityPeriod,
                    CurrentPrice = x.CurrentPrice,
                    FaceValue = x.FaceValue,
                    Rating = x.Rating,
                    Currency = x.Currency,
                    Coupons = x.Coupons != null ?
                        x.Coupons.Select(d => new CouponDto
                        {
                            Id = d.Id,
                            BondId = d.ParentId,
                            Currency = d.Currency,
                            Value = d.Value
                        }).ToList()
                        : new List<CouponDto>()
                }).ToList();

            return bondCardDtoList;
        }

        /// <summary>
        /// Получить карточку облигации по Id
        /// </summary>
        /// <param name="id"> ID карточки облигации</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BondCardDto?> GetByIdAsync(Guid id)
        {
            var bondCard = await _bondCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (bondCard == null)
                return null;

            var bondCardDto = new BondCardDto()
            {
                Id = bondCard.Id,
                Ticker = bondCard.Ticker,
                Name = bondCard.Name,
                Board = bondCard.Board,
                Description = bondCard.Description,
                Currency = bondCard.Currency,
                MaturityPeriod = bondCard.MaturityPeriod,
                CurrentPrice = bondCard.CurrentPrice,
                FaceValue = bondCard.FaceValue,
                Rating = bondCard.Rating,
                Coupons = bondCard.Coupons != null ?
                        bondCard.Coupons.Select(d => new CouponDto
                        {
                            Id = d.Id,
                            BondId = d.ParentId,
                            Currency = d.Currency,
                            Value = d.Value
                        }).ToList()
                        : new List<CouponDto>()
            };
            return bondCardDto;
        }

        /// <summary>
        /// Получить карточку облигации по Id со связанными объектами
        /// </summary>
        /// <param name="id"> ID карточки облигации</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BondCardDto?> GetByIdWithLinkedItemsAsync(Guid id)
        {
            var bondCard = await _bondCardRepository.GetByIdWithLinkedItemsAsync(id, CancellationToken.None, b => b.Coupons);

            if (bondCard == null)
                return null;

            var bondCardDto = new BondCardDto()
            {
                Id = bondCard.Id,
                Ticker = bondCard.Ticker,
                Name = bondCard.Name,
                Board = bondCard.Board,
                Description = bondCard.Description,
                Currency = bondCard.Currency,
                MaturityPeriod = bondCard.MaturityPeriod,
                CurrentPrice = bondCard.CurrentPrice,
                FaceValue = bondCard.FaceValue,
                Rating = bondCard.Rating,
                Coupons = bondCard.Coupons != null ?
                        bondCard.Coupons.Select(d => new CouponDto
                        {
                            Id = d.Id,
                            BondId = d.ParentId,
                            Currency = d.Currency,
                            Value = d.Value
                        }).ToList()
                        : new List<CouponDto>()
            };
            return bondCardDto;
        }

        /// <summary>
        /// Получить неполные данные карточки облигации по Id
        /// </summary>
        /// <param name="id">ID карточки облигации</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BondCardShortDto?> GetShortByIdAsync(Guid id)
        {
            var bondCard = await _bondCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (bondCard == null)
                return null;

            var bondCardShortDto = new BondCardShortDto()
            {
                Id = bondCard.Id,
                Ticker = bondCard.Ticker,
                Name = bondCard.Name,
                Board = bondCard.Board,
                Description = bondCard.Description,
                MaturityPeriod = bondCard.MaturityPeriod.ToString(),
                Currency = bondCard.Currency,
                CurrentPrice = bondCard.CurrentPrice,
                FaceValue = bondCard.FaceValue,
                Rating = bondCard.Rating
            };
            return bondCardShortDto;
        }

        /// <summary>
        /// Добавить новую карточку облигации
        /// </summary>
        /// <param name="creatingBondCardDto"></param>
        /// <returns></returns>
        public async Task<Guid> CreateAsync(CreatingBondCardDto creatingBondCardDto)
        {
            var bondCard = new BondCard()
            {
                Id = Guid.NewGuid(),
                Ticker = creatingBondCardDto.Ticker,
                Name = creatingBondCardDto.Name,
                Board = creatingBondCardDto.Board,
                Description = creatingBondCardDto.Description,
                Currency = creatingBondCardDto.Currency,
                MaturityPeriod = creatingBondCardDto.MaturityPeriod,
                CurrentPrice = 0m,
                FaceValue = creatingBondCardDto.FaceValue,
                Rating = creatingBondCardDto.Rating,
            };

            var createdBondCard = await _bondCardRepository.AddAsync(bondCard);
            return createdBondCard.Id;
        }

        /// <summary>
        /// Обновить существующую карточку облигации
        /// </summary>
        /// <param name="updatingBondCardDto">Измененная карточка облигации</param>
        /// <returns></returns>
        public async Task UpdateAsync(UpdatingBondCardDto updatingBondCardDto)
        {
            var bondCard = await _bondCardRepository.GetByIdAsync(updatingBondCardDto.Id, CancellationToken.None);
            if (bondCard == null)
                return;
            bondCard.Ticker = updatingBondCardDto.Ticker;
            bondCard.Name = updatingBondCardDto.Name;
            bondCard.Description = updatingBondCardDto.Description;
            bondCard.MaturityPeriod = updatingBondCardDto.MaturityPeriod;
            bondCard.Rating = updatingBondCardDto.Rating;
            await _bondCardRepository.UpdateAsync(bondCard);
        }

        /// <summary>
        /// Обновить цену для всех облигаций
        /// </summary>
        /// <returns></returns>
        public async Task UpdateBondCardPricesAsync()
        {
            var bondCards = await _bondCardRepository.GetAllAsync(CancellationToken.None);
            var CardsAndPrices = new ConcurrentDictionary<string, decimal?>();
            //параллелим определение цены актива с помощью МосБиржи. Число параллельных потоков - 5
            using var semaphore = new SemaphoreSlim(5);
            var tasks = bondCards.Select(async card =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var priceRequest = await _stockPriceService.GetCurrentPriceAsync(card.Ticker, "bonds", card.Board, CancellationToken.None);
                    if (priceRequest != null)
                    {
                        //Цена облигации публикуется в процентах от номинала
                        CardsAndPrices[card.Ticker] = (decimal)priceRequest * card.FaceValue / 100;
                    }
                }
                catch
                {
                    _logger.LogError($"Failed to get the actual price for BondCard '{card.Ticker}'.");
                }
                finally
                {
                    semaphore.Release();
                }
            });
            await Task.WhenAll(tasks);
            //Обновляем БД последовательно (т.к. работаем с одним DbContext)
            foreach (var card in bondCards)
            {
                if (CardsAndPrices.TryGetValue(card.Ticker, out var price) && price != null)
                {
                    card.CurrentPrice = (decimal)price;
                    await _bondCardRepository.UpdateAsync(card);
                }
            }
        }

        /// <summary>
        /// Удалить карточку облигации
        /// </summary>
        /// <param name="id">Id карточки облигации</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DeleteAsync(Guid id)
        {
            await _bondCardRepository.DeleteAsync(id);
        }
    }
}
