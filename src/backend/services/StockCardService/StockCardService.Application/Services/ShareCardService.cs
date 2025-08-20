using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Multiplier;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    /// <summary>
    /// Сервис работы с карточкой акции
    /// </summary>
    public class ShareCardService : IShareCardService
    {
        private readonly IRepository<ShareCard, Guid> _shareCardRepository;

        public ShareCardService(IRepository<ShareCard, Guid> shareCardRepository)
        {
            _shareCardRepository = shareCardRepository;
        }

        /// <summary>
        /// Получить все карточки акции
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ShareCardDto>> GetAllAsync()
        {
            var shareCards = await _shareCardRepository.GetAllAsync(CancellationToken.None);
            var shareCardDtoList = shareCards.Select(x =>
                new ShareCardDto()
                {
                    Id = x.Id,
                    Ticker = x.Ticker,
                    Name = x.Name,
                    Description = x.Description,
                    Currency = x.Currency,
                    CurrentPrice = x.CurrentPrice,
                    Multipliers = x.Multipliers != null ?
                        x.Multipliers.Select(m => new MultiplierDto
                        {
                            Id = m.Id,
                            Name = m.Name
                        }).ToList()
                        : new List<MultiplierDto>(),
                    Dividends = x.Dividends != null ? 
                        x.Dividends.Select(d => new DividendDto
                        {
                            Id = d.Id,
                            ShareCardId = d.ParentId,
                            Currency = d.Currency,
                            CutOffDate = d.CutOffDate,
                            Period = d.Period,
                            Value = d.Value
                        }).ToList()
                        : new List<DividendDto>()
                }).ToList();

            return shareCardDtoList;
        }

        /// <summary>
        /// Получить карточку акции по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ShareCardDto?> GetByIdAsync(Guid id)
        {
            var shareCard = await _shareCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (shareCard == null)
                return null;
            //var customersPreferencesList = (await _shareCardRepository.GetByIdWithPreferenceAsync(id)).CustomerPreferences.Select(cp => cp.Preference).ToList();

            var shareCardDto = new ShareCardDto()
            {
                Id = shareCard.Id,
                Ticker = shareCard.Ticker,
                Name = shareCard.Name,
                Description = shareCard.Description,
                Currency = shareCard.Currency,
                CurrentPrice = shareCard.CurrentPrice,
                Multipliers = shareCard.Multipliers != null ?
                    shareCard.Multipliers.Select(m => new MultiplierDto
                    {
                        Id = m.Id,
                        Name = m.Name
                    }).ToList()
                    : new List<MultiplierDto>(),
                Dividends = shareCard.Dividends != null ?
                    shareCard.Dividends.Select(d => new DividendDto
                    {
                        Id = d.Id,
                        ShareCardId = d.Id,
                        CutOffDate = d.CutOffDate,
                        Currency = d.Currency,
                        Period = d.Period,
                        Value = d.Value
                    }).ToList()
                    : new List<DividendDto>()
            };
            return shareCardDto;
        }

        public async Task<ShareCardDto?> GetByIdWithLinkedItemsAsync(Guid id)
        {
            var shareCard = await _shareCardRepository.GetByIdWithLinkedItemsAsync(id, CancellationToken.None, sh => sh.Dividends);

            if (shareCard == null)
                return null;

            var shareCardDto = new ShareCardDto()
            {
                Id = shareCard.Id,
                Ticker = shareCard.Ticker,
                Name = shareCard.Name,
                Description = shareCard.Description,
                Currency = shareCard.Currency,
                CurrentPrice = shareCard.CurrentPrice,
                Multipliers = shareCard.Multipliers != null ?
                    shareCard.Multipliers.Select(m => new MultiplierDto
                    {
                        Id = m.Id,
                        Name = m.Name
                    }).ToList()
                    : new List<MultiplierDto>(),
                Dividends = shareCard.Dividends != null ?
                    shareCard.Dividends.Select(d => new DividendDto
                    {
                        Id = d.Id,
                        ShareCardId = d.Id,
                        CutOffDate = d.CutOffDate,
                        Currency = d.Currency,
                        Period = d.Period,
                        Value = d.Value
                    }).ToList()
                    : new List<DividendDto>()
            };
            return shareCardDto;
        }

        /// <summary>
        /// Получить неполные данные карточки акции по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ShareCardShortDto?> GetShortByIdAsync(Guid id)
        {
            var shareCard = await _shareCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (shareCard == null)
                return null;

            var shareCardDto = new ShareCardShortDto()
            {
                Id = shareCard.Id,
                Ticker = shareCard.Ticker,
                Name = shareCard.Name,
                Description = shareCard.Description,
                CurrentPrice = shareCard.CurrentPrice,
                Currency = shareCard.Currency
            };
            return shareCardDto;
        }

        /// <summary>
        /// Создать новую карточку акции
        /// </summary>
        /// <param name="creatingShareCardDto"></param>
        /// <returns></returns>
        public async Task<Guid> CreateAsync(CreatingShareCardDto creatingShareCardDto)
        {
            var shareCard = new ShareCard()
            {
                Id = Guid.NewGuid(),
                Ticker = creatingShareCardDto.Ticker,
                Name = creatingShareCardDto.Name,
                Description = creatingShareCardDto.Description,
                CurrentPrice = 0m,
                Currency = creatingShareCardDto.Currency
            };

            var createdShareCard = await _shareCardRepository.AddAsync(shareCard);
            return createdShareCard.Id;
        }

        /// <summary>
        /// Удалить карточку акции
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            await _shareCardRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Обновить карточку акции
        /// </summary>
        /// <param name="updatingShareCardDto"></param>
        /// <returns></returns>
        public async Task UpdateAsync(UpdatingShareCardDto updatingShareCardDto)
        {
            var shareCard = await _shareCardRepository.GetByIdAsync(updatingShareCardDto.Id, CancellationToken.None);
            if (shareCard == null)
                return;
            shareCard.Id = updatingShareCardDto.Id;
            shareCard.Ticker = updatingShareCardDto.Ticker;
            shareCard.Name = updatingShareCardDto.Name;
            shareCard.Description = updatingShareCardDto.Description;
            shareCard.CurrentPrice = updatingShareCardDto.CurrentPrice;
            await _shareCardRepository.UpdateAsync(shareCard);
        }
    }
}
