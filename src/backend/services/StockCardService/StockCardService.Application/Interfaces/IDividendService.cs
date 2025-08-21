using StockMarketAssistant.StockCardService.Application.DTOs._01sub_Dividend;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса по работе с дивидендами акции
    /// </summary>
    public interface IDividendService
    {
        /// <summary>
        /// Получить список всех дивидендов всех акций
        /// </summary>
        /// <returns>Список всех дивидендов всех акций</returns>
        Task<IEnumerable<DividendDto>> GetAllAsync();
        
        /// <summary>
        /// Получить список всех дивидендов одной акции
        /// </summary>
        /// <param name="id">Id акции</param>
        /// <returns>Список дивидендов указанной акции</returns>
        Task<IEnumerable<DividendDto>> GetAllByShareCardIdAsync(Guid id);

        /// <summary>
        /// Получить карточку дивидендов по Id
        /// </summary>
        /// <param name="id">Идентификатор дивиденов акции</param>
        /// <returns>DTO дивиденда акции</returns>
        Task<DividendDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Создать дивиденд
        /// </summary>
        /// <param name="creatingDividendDto">DTO создаваемого дивиденда акции</param>
        Task<Guid> CreateAsync(CreatingDividendDto creatingDividendDto);

        /// <summary>
        /// Обновить дивиденд
        /// </summary>
        /// <param name="updatingDividendDto">DTO редактируемого дивиденда акции</param>
        Task UpdateAsync(UpdatingDividendDto updatingDividendDto);

        /// <summary>
        /// Удалить дивиденд
        /// </summary>
        /// <param name="id">Идентификатор дивиденда</param>
        Task DeleteAsync(Guid id);
    }
}
