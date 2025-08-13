using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Domain.Entities;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    /// <summary>
    /// Сервис работы с дивидендами акций
    /// </summary>
    public class DividendService : IDividendService
    {
        private readonly ISubRepository<Dividend, Guid> _dividendRepository;

        public DividendService(ISubRepository<Dividend, Guid> dividendRepository)
        {
            _dividendRepository = dividendRepository;
        }

        /// <summary>
        /// Получить все дивиденды всех акций
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DividendDto>> GetAllAsync()
        {
            var dividends = await _dividendRepository.GetAllAsync(CancellationToken.None);
            var dividendModelList = dividends.Select(x =>
                new DividendDto()
                {
                    Id = x.Id,
                    ShareCardId = x.ParentId,
                    CuttOffDate = x.CuttOffDate,
                    Period = x.Period,
                    Currency = x.Currency,
                    Value = x.Value
                }).ToList();

            return dividendModelList;
        }

        /// <summary>
        /// Получить список всех дивидендов одной акции
        /// </summary>
        /// <param name="id">Id акции</param>
        /// <returns>Список дивидендов указанной акции</returns>
        public async Task<IEnumerable<DividendDto>> GetAllByShareCardIdAsync(Guid id)
        {
            var dividends = await _dividendRepository.GetAllByParentIdAsync(id, CancellationToken.None);
            var dividendModelList = dividends.Select(x =>
                new DividendDto()
                {
                    Id = x.Id,
                    ShareCardId = x.ParentId,
                    CuttOffDate = x.CuttOffDate,
                    Period = x.Period,
                    Currency = x.Currency,
                    Value = x.Value
                }).ToList();

            return dividendModelList;
        }

        /// <summary>
        /// Получить дивиденд по Id
        /// </summary>
        /// <param name="id">ID дивиденда</param>
        /// <returns></returns>
        public async Task<DividendDto?> GetByIdAsync(Guid id)
        {
            var dividend = await _dividendRepository.GetByIdAsync(id, CancellationToken.None);

            if (dividend == null)
                return null;

            var dividendDto = new DividendDto()
            {
                Id = dividend.Id,
                ShareCardId = dividend.ParentId,
                CuttOffDate = dividend.CuttOffDate,
                Period = dividend.Period,
                Currency = dividend.Currency,
                Value = dividend.Value
            };
            return dividendDto;
        }

        /// <summary>
        /// Добавить новый дивиденд
        /// </summary>
        /// <param name="creatingDividendDto">Dto создаваемого дивиденда</param>
        /// <returns></returns>
        public async Task<Guid> CreateAsync(CreatingDividendDto creatingDividendDto)
        {
            var dividend = new Dividend()
            {
                Id = Guid.NewGuid(),
                ParentId = creatingDividendDto.ShareCardId,
                CuttOffDate = creatingDividendDto.CuttOffDate,
                Period = creatingDividendDto.Period,
                Currency = creatingDividendDto.Currency,
                Value = creatingDividendDto.Value
            };

            var createdDividend = await _dividendRepository.AddAsync(dividend);
            return createdDividend.Id;
        }

        /// <summary>
        /// Обновить информацию о дивиденде
        /// </summary>
        /// <param name="updatingDividendDto">Dto Измененненного дивиденда</param>
        /// <returns></returns>
        public async Task UpdateAsync(UpdatingDividendDto updatingDividendDto)
        {
            var dividend = await _dividendRepository.GetByIdAsync(updatingDividendDto.Id, CancellationToken.None);
            if (dividend == null)
                return;
            dividend.CuttOffDate = updatingDividendDto.CuttOffDate;
            dividend.Value = updatingDividendDto.Value;
            await _dividendRepository.UpdateAsync(dividend);
        }

        /// <summary>
        /// Удалить дивиденд
        /// </summary>
        /// <param name="id">Id дивиденда</param>
        /// <returns></returns>
        public async Task DeleteAsync(Guid id)
        {
            await _dividendRepository.DeleteAsync(id);
        }
    }
}
