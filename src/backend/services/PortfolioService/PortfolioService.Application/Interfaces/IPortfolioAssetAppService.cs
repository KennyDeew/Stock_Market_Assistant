using StockMarketAssistant.PortfolioService.Application.DTOs;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса работы с финансовыми активами в портфеле
    /// </summary>
    public interface IPortfolioAssetAppService
    {
        /// <summary>
        /// Получить актив
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <returns>DTO для актива в портфеле</returns>
        Task<PortfolioAssetDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Существует ли актив
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Создать актив
        /// </summary>
        /// <param name="creatingCourseDto">DTO создаваемого актива в портфеле</param>
        Task<Guid> CreateAsync(CreatingPortfolioAssetDto creatingPortfolioAssetDto);

        /// <summary>
        /// Обновить актив
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        /// <param name="updatingPortfolioDto">DTO редактируемого актива в портфеле</param>
        Task UpdateAsync(Guid id, UpdatingPortfolioAssetDto updatingPortfolioAssetDto);

        /// <summary>
        /// Удалить актив
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        Task DeleteAsync(Guid id);

    }
}
