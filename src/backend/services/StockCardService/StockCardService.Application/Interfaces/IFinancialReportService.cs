using StockMarketAssistant.StockCardService.Application.DTOs._01sub_FinancialReport;

namespace StockMarketAssistant.StockCardService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса по работе с отчетами
    /// </summary>
    public interface IFinancialReportService
    {
        /// <summary>
        /// Получить список всех финансовых отчетов
        /// </summary>
        /// <returns>Список финансовых отчетов всех акций</returns>
        Task<IEnumerable<FinancialReportDto>> GetAllAsync();

        /// <summary>
        /// Получить список финансовых отчетов одной акции
        /// </summary>
        /// <param name="id">Id акции</param>
        /// <returns>Список финансовых отчетов указанной акции</returns>
        Task<IEnumerable<FinancialReportDto>> GetAllByShareCardIdAsync(Guid id);

        /// <summary>
        /// Получить карточку финансового отчета по Id
        /// </summary>
        /// <param name="id">Идентификатор финансового отчета акции</param>
        /// <returns>DTO финансового отчета акции</returns>
        Task<FinancialReportDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Добавить финансовый отчет
        /// </summary>
        /// <param name="сreatingFinancialReportDto">DTO создаваемого финансового отчета акции</param>
        Task<Guid> CreateAsync(CreatingFinancialReportDto сreatingFinancialReportDto);

        /// <summary>
        /// Обновить финансовый отчет
        /// </summary>
        /// <param name="updatingFinancialReportDto">DTO редактируемого финансового отчета акции</param>
        Task UpdateAsync(UpdatingFinancialReportDto updatingFinancialReportDto);

        /// <summary>
        /// Удалить финансовый отчет
        /// </summary>
        /// <param name="id">Идентификатор финансового отчета</param>
        Task DeleteAsync(Guid id);
    }
}
