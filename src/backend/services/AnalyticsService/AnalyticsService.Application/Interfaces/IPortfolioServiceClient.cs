using StockMarketAssistant.AnalyticsService.Application.DTOs;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс HTTP клиента для работы с PortfolioService
    /// </summary>
    public interface IPortfolioServiceClient
    {
        /// <summary>
        /// Получить историю транзакций портфеля за период
        /// Кэшируется на 5 минут
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля</param>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>История транзакций или null, если портфель не найден (404)</returns>
        Task<PortfolioHistoryDto?> GetHistoryAsync(
            Guid portfolioId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить текущее состояние портфеля (активы)
        /// НЕ кэшируется
        /// </summary>
        /// <param name="portfolioId">Идентификатор портфеля</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Текущее состояние портфеля или null, если портфель не найден (404)</returns>
        Task<PortfolioStateDto?> GetCurrentStateAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить состояние нескольких портфелей
        /// НЕ кэшируется
        /// </summary>
        /// <param name="portfolioIds">Идентификаторы портфелей</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Словарь состояний портфелей (ключ - PortfolioId)</returns>
        Task<Dictionary<Guid, PortfolioStateDto>> GetMultipleStatesAsync(
            IEnumerable<Guid> portfolioIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Создать новый портфель
        /// </summary>
        /// <param name="userId">Идентификатор пользователя-владельца портфеля</param>
        /// <param name="name">Наименование портфеля</param>
        /// <param name="currency">Валюта портфеля (RUB, USD и т.д.)</param>
        /// <param name="isPrivate">Скрыть портфель из публичной статистики</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Идентификатор созданного портфеля или Guid.Empty при ошибке</returns>
        Task<Guid> CreatePortfolioAsync(
            Guid userId,
            string name,
            string currency = "RUB",
            bool isPrivate = false,
            CancellationToken cancellationToken = default);
    }
}

