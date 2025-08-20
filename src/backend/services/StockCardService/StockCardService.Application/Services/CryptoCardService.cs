using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._03_CryptoCard;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    public class CryptoCardService : ICryptoCardService
    {
        private readonly IRepository<CryptoCard, Guid> _cryptoCardRepository;

        public CryptoCardService(IRepository<CryptoCard, Guid> cryptoCardRepository)
        {
            _cryptoCardRepository = cryptoCardRepository;
        }

        /// <summary>
        /// Получить все карточки криптовалют
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<CryptoCardDto>> GetAllAsync()
        {
            var cryptoCards = await _cryptoCardRepository.GetAllAsync(CancellationToken.None);
            var cryptoCardModelList = cryptoCards.Select(x =>
                new CryptoCardDto()
                {
                    Id = x.Id,
                    Ticker = x.Ticker,
                    Name = x.Name,
                    CurrentPrice = x.CurrentPrice,
                    Description = x.Description
                }).ToList();

            return cryptoCardModelList;
        }

        /// <summary>
        /// Получить карточку криптовалюты по Id
        /// </summary>
        /// <param name="id">ID карточки криптовалюты</param>
        /// <returns></returns>
        public async Task<CryptoCardDto?> GetByIdAsync(Guid id)
        {
            var cryptoCard = await _cryptoCardRepository.GetByIdAsync(id, CancellationToken.None);

            if (cryptoCard == null)
                return null;

            var cryptoCardDto = new CryptoCardDto()
            {
                Id = cryptoCard.Id,
                Ticker = cryptoCard.Ticker,
                Name = cryptoCard.Name,
                CurrentPrice = cryptoCard.CurrentPrice,
                Description = cryptoCard.Description
            };
            return cryptoCardDto;
        }

        /// <summary>
        /// Добавить новую карточку криптовалюты
        /// </summary>
        /// <param name="creatingCryptoCardDto">Dto создаваемой карточки криптовалюты</param>
        /// <returns></returns>
        public async Task<Guid> CreateAsync(CreatingCryptoCardDto creatingCryptoCardDto)
        {
            var cryptoCard = new CryptoCard()
            {
                Id = Guid.NewGuid(),
                Ticker = creatingCryptoCardDto.Ticker,
                Name = creatingCryptoCardDto.Name,
                CurrentPrice = 0m,
                Description = creatingCryptoCardDto.Description
            };

            var createdCryptoCard = await _cryptoCardRepository.AddAsync(cryptoCard);
            return createdCryptoCard.Id;
        }

        /// <summary>
        /// Обновить существующую карточку криптовалюты
        /// </summary>
        /// <param name="updatingCryptoCardDto">Измененная карточка криптовалюты</param>
        /// <returns></returns>
        public async Task UpdateAsync(UpdatingCryptoCardDto updatingCryptoCardDto)
        {
            var cryptoCard = await _cryptoCardRepository.GetByIdAsync(updatingCryptoCardDto.Id, CancellationToken.None);
            if (cryptoCard == null)
                return;
            cryptoCard.Ticker = updatingCryptoCardDto.Ticker;
            cryptoCard.Name = updatingCryptoCardDto.Name;
            cryptoCard.Description = updatingCryptoCardDto.Description;
            cryptoCard.CurrentPrice = updatingCryptoCardDto.CurrentPrice;
            await _cryptoCardRepository.UpdateAsync(cryptoCard);
        }

        /// <summary>
        /// Удалить карточку криптовалюты
        /// </summary>
        /// <param name="id">Id карточки криптовалюты</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            await _cryptoCardRepository.DeleteAsync(id);
        }
    }
}
