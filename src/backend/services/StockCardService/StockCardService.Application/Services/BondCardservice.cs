using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard;
using StockMarketAssistant.StockCardService.Application.DTOs._02sub_Coupon;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    /// <summary>
    /// Сервис работы с карточкой облигации
    /// </summary>
    public class BondCardservice : IBondCardService
    {
        private readonly IRepository<BondCard, Guid> _bondCardRepository;

        public BondCardservice(IRepository<BondCard, Guid> bondCardRepository)
        {
            _bondCardRepository = bondCardRepository;
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
                    Description = x.Description,
                    MaturityPeriod = x.MaturityPeriod,
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
                Description = bondCard.Description,
                Currency = bondCard.Currency,
                MaturityPeriod = bondCard.MaturityPeriod,
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
                Description = bondCard.Description,
                Currency = bondCard.Currency,
                MaturityPeriod = bondCard.MaturityPeriod,
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
                Description = bondCard.Description,
                MaturityPeriod = bondCard.MaturityPeriod.ToString(),
                Currency = bondCard.Currency
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
                Description = creatingBondCardDto.Description,
                Currency = creatingBondCardDto.Currency,
                MaturityPeriod = creatingBondCardDto.MaturityPeriod
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
            await _bondCardRepository.UpdateAsync(bondCard);
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
