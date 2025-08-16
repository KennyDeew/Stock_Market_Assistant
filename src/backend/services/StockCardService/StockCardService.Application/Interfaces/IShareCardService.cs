using StockMarketAssistant.StockCardService.Application.DTOs._01_ShareCard;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса по работе с карточкой акции
    /// </summary>
    public interface IShareCardService
    {
        /// <summary>
        /// Получить список всех карточек акций
        /// </summary>
        /// <returns>Список всех карточек акций</returns>
        Task<IEnumerable<ShareCardDto>> GetAllAsync();

        /// <summary>
        /// Получить карточку акции по Id
        /// </summary>
        /// <param name="id">Идентификатор карточки акции</param>
        /// <returns>DTO карточки акции</returns>
        Task<ShareCardDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Получить карточку акции по Id со связанными объектами
        /// </summary>
        /// <param name="id">Идентификатор карточки акции</param>
        /// <returns>DTO карточки акции</returns>
        Task<ShareCardDto?> GetByIdWithLinkedItemsAsync(Guid id);

        /// <summary>
        /// Получить неполные данные карточки акции по Id
        /// </summary>
        /// <param name="id">Идентификатор карточки акции</param>
        /// <returns>DTO карточки акции</returns>
        Task<ShareCardShortDto?> GetShortByIdAsync(Guid id);

        /// <summary>
        /// Создать карточку акции
        /// </summary>
        /// <param name="creatingShareCardDto">DTO создаваемой карточки акции</param>
        Task<Guid> CreateAsync(CreatingShareCardDto creatingShareCardDto);

        /// <summary>
        /// Обновить карточку акции
        /// </summary>
        /// <param name="updatingShareCardDto">DTO редактируемой карточки акции</param>
        Task UpdateAsync(UpdatingShareCardDto updatingShareCardDto);

        /// <summary>
        /// Удалить карточку акции
        /// </summary>
        /// <param name="id">Идентификатор карточки акции</param>
        Task DeleteAsync(Guid id);
    }
}
