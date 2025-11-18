using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Domain.Enums;
using StockMarketAssistant.SharedLibrary.Enums;

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
        /// Создать актив с начальной транзакцией
        /// </summary>
        /// <param name="creatingCourseDto">DTO создаваемого актива в портфеле</param>
        Task<PortfolioAssetDto> CreateAsync(CreatingPortfolioAssetDto dto);

        /// <summary>
        /// Удалить актив
        /// </summary>
        /// <param name="id">Идентификатор актива</param>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Получить транзакцию по идентификатору
        /// </summary>
        Task<PortfolioAssetTransactionDto?> GetAssetTransactionByIdAsync(Guid transactionId);

        /// <summary>
        /// Получить все транзакции актива
        /// </summary>
        Task<IEnumerable<PortfolioAssetTransactionDto>> GetAssetTransactionsByAssetIdAsync(Guid assetId);

        /// <summary>
        /// Получить транзакции актива за период
        /// </summary>
        Task<IEnumerable<PortfolioAssetTransactionDto>> GetAssetTransactionsByAssetIdAndPeriodAsync(
            Guid assetId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Добавить транзакцию покупки/продажи к активу
        /// </summary>
        /// <param name="assetId">Идентификатор актива</param>
        /// <param name="dto">DTO добавляемой транзакции</param>
        Task<PortfolioAssetTransactionDto> AddAssetTransactionAsync(Guid assetId, CreatingPortfolioAssetTransactionDto dto);

        /// <summary>
        /// Обновить транзакцию актива
        /// </summary>
        /// <param name="id">Идентификатор транзакции</param>
        /// <param name="dto">DTO редактируемой транзакции</param>
        Task UpdateAssetTransactionAsync(Guid id, UpdatingPortfolioAssetTransactionDto dto);

        /// <summary>
        /// Удалить транзакцию актива
        /// </summary>
        Task<bool> DeleteAssetTransactionAsync(Guid transactionId);

        /// <summary>
        /// Получить информацию по доходности актива
        /// </summary>
        /// <param name="assetId">Идентификатор актива</param>
        /// <param name="startDate">Начальная дата периода, за который рассчитывается доходность</param>
        /// <param name="endDate">Конечная дата периода, за который рассчитывается доходность</param>
        /// <returns></returns>
        Task<PortfolioAssetProfitLossDto?> GetAssetProfitLossAsync(Guid assetId, CalculationType calculationType);

        /// <summary>
        /// Получить информацию о ценной бумаге актива из внешнего сервиса StockCardService
        /// </summary>
        /// <param name="assetType">Тип финансового актива</param>
        /// <param name="stockCardId">Идентификатор ценной бумаги</param>
        /// <param name="toRetrieveCurrentPrice">Требуется ли считывать текущую цену по ценной бумаге</param>
        /// <returns></returns>
        Task<StockCardInfoDto> GetStockCardInfoAsync(PortfolioAssetType assetType, Guid stockCardId, bool toRetrieveCurrentPrice);
    }
}
