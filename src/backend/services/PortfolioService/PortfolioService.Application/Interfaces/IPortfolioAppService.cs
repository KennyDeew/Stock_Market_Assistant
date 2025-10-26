using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.SharedLibrary.Enums;

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
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Получить расчет доходности портфеля по его ID с детализацией по активам
        /// </summary>
        /// <param name="portfolioId">Уникальный идентификатор портфеля</param>
        /// <param name="calculationType">Тип расчета доходности (по умолчанию - текущая доходность)</param>
        /// <returns>DTO с расчетом доходности портфеля и детализацией по активам</returns>
        Task<PortfolioProfitLossDto?> GetPortfolioProfitLossAsync(Guid id, CalculationType calculationType = CalculationType.Current);

        /// <summary>
        /// Получить расчет доходности по всем активам портфеля
        /// </summary>
        /// <param name="portfolioId">Уникальный идентификатор портфеля</param>
        /// <param name="calculationType">Тип расчета доходности (по умолчанию - текущая доходность)</param>
        /// <returns>Коллекция DTO с расчетами доходности по каждому активу портфеля</returns>
        Task<IEnumerable<PortfolioAssetProfitLossItemDto>> GetPortfolioAssetsProfitLossAsync(Guid id, CalculationType calculationType = CalculationType.Current);
    }
}
