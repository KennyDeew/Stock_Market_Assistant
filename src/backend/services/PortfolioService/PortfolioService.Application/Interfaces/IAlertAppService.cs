using StockMarketAssistant.PortfolioService.Application.DTOs;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces
{
    /// <summary>
    /// Сервис для работы с оповещениями
    /// </summary>
    public interface IAlertAppService
    {
        /// <summary>
        /// Обработать ожидающие оповещения
        /// </summary>
        Task ProcessPendingAlertsAsync();

        /// <summary>
        /// Создать новое ововещение
        /// </summary>
        /// <param name="alert">Уведомление</param>
        Task<Guid> CreateAsync(CreatingAlertDto alert);

        /// <summary>
        /// Получить все необработанные оповещения для указанного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        Task<IEnumerable<AlertDto>> GetAllPendingAlertsAsync(Guid userId);

        /// <summary>
        /// Получить оповещение
        /// </summary>
        /// <param name="id">Идентификатор оповещения</param>
        /// <returns>DTO портфеля</returns>
        Task<AlertDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Удалить оповещение
        /// </summary>
        /// <param name="id">Идентификатор оповещения</param>
        Task<bool> DeleteAsync(Guid id);
    }
}