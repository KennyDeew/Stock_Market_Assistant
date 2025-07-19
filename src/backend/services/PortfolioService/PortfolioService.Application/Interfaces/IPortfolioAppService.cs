using StockMarketAssistant.PortfolioService.Application.DTOs;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса работы с портфелями ценных бумаг
    /// </summary>
    public interface IPortfolioAppService
    {
        /// <summary>
        /// Получить список всех портфелей
        /// </summary>
        /// <returns>Список портфелей</returns>
        Task<IEnumerable<PortfolioDto>> GetAllAsync();

        /// <summary>
        /// Получить портфель
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <returns>DTO портфеля</returns>
        Task<PortfolioDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Существует ли портфель
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Создать портфель
        /// </summary>
        /// <param name="creatingCourseDto">DTO создаваемого портфеля</param>
        Task<Guid> CreateAsync(CreatingPortfolioDto creatingPortfolioDto);

        /// <summary>
        /// Обновить портфель
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        /// <param name="updatingPortfolioDto">DTO редактируемого портфеля</param>
        Task UpdateAsync(Guid id, UpdatingPortfolioDto updatingPortfolioDto);

        /// <summary>
        /// Удалить портфель
        /// </summary>
        /// <param name="id">Идентификатор портфеля</param>
        Task DeleteAsync(Guid id);

    }
}
