using StockMarketAssistant.StockCardService.Application.DTOs._02_BondCard;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// интерфейс сервиса по работе с карточкой облигации
    /// </summary>
    public interface IBondCardService
    {
        /// <summary>
        /// Получить список всех карточек облигаций
        /// </summary>
        /// <returns>Список всех карточек облигаций</returns>
        Task<IEnumerable<BondCardDto>> GetAllAsync();

        /// <summary>
        /// Получить карточку облигации по Id
        /// </summary>
        /// <param name="id">Идентификатор карточки облигации</param>
        /// <returns>DTO карточки облигации</returns>
        Task<BondCardDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Получить карточку облигации по Id со связанными объектами
        /// </summary>
        /// <param name="id">Идентификатор карточки облигации</param>
        /// <returns>DTO карточки облигации</returns>
        Task<BondCardDto?> GetByIdWithLinkedItemsAsync(Guid id);

        /// <summary>
        /// Получить неполные данные карточки облигации по Id
        /// </summary>
        /// <param name="id">Идентификатор карточки облигации</param>
        /// <returns>DTO карточки облигации</returns>
        Task<BondCardShortDto?> GetShortByIdAsync(Guid id);

        /// <summary>
        /// Создать карточку облигации
        /// </summary>
        /// <param name="creatingBondCardDto">DTO создаваемой карточки облигации</param>
        Task<Guid> CreateAsync(CreatingBondCardDto creatingBondCardDto);

        /// <summary>
        /// Обновить карточку облигации
        /// </summary>
        /// <param name="updatingBondCardDto">DTO редактируемой карточки облигации</param>
        Task UpdateAsync(UpdatingBondCardDto updatingBondCardDto);

        /// <summary>
        /// Обновить цену для всех облигаций
        /// </summary>
        /// <returns></returns>
        Task UpdateBondCardPricesAsync();

        /// <summary>
        /// Удалить карточку облигации
        /// </summary>
        /// <param name="id">Идентификатор карточки облигации</param>
        Task DeleteAsync(Guid id);
    }
}
