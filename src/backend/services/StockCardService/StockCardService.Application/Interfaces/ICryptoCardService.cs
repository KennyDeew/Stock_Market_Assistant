using StockMarketAssistant.StockCardService.Application.DTOs._03_CryptoCard;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// интерфейс сервиса по работе с карточкой криптовалюты
    /// </summary>
    public interface ICryptoCardService
    {
        /// <summary>
        /// Получить список всех карточек акций
        /// </summary>
        /// <returns>Список всех карточек акций</returns>
        Task<IEnumerable<CryptoCardDto>> GetAllAsync();

        /// <summary>
        /// Получить карточку криптовалюты по Id
        /// </summary>
        /// <param name="id">Идентификатор карточки криптовалюты</param>
        /// <returns>DTO карточки криптовалюты</returns>
        Task<CryptoCardDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Создать карточку криптовалюты
        /// </summary>
        /// <param name="creatingCryptoCardDto">DTO создаваемой карточки криптовалюты</param>
        Task<Guid> CreateAsync(CreatingCryptoCardDto creatingCryptoCardDto);

        /// <summary>
        /// Обновить карточку криптовалюты
        /// </summary>
        /// <param name="updatingCryptoCardDto">DTO редактируемой карточки криптовалюты</param>
        Task UpdateAsync(UpdatingCryptoCardDto updatingCryptoCardDto);

        /// <summary>
        /// Удалить карточку криптовалюты
        /// </summary>
        /// <param name="id">Идентификатор карточки криптовалюты</param>
        Task DeleteAsync(Guid id);
    }
}
