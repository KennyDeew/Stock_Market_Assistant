using StockMarketAssistant.AnalyticsService.Application.DTOs;

namespace StockMarketAssistant.AnalyticsService.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса потребления транзакций из Kafka
    /// </summary>
    public interface ITransactionConsumerService
    {
        /// <summary>
        /// Запускает потребление транзакций из Kafka
        /// </summary>
        /// <returns>Задача запуска</returns>
        Task StartConsumingAsync();

        /// <summary>
        /// Останавливает потребление транзакций из Kafka
        /// </summary>
        /// <returns>Задача остановки</returns>
        Task StopConsumingAsync();

        /// <summary>
        /// Обрабатывает полученную транзакцию
        /// </summary>
        /// <param name="transaction">Транзакция для обработки</param>
        /// <returns>Задача обработки</returns>
        Task ProcessTransactionAsync(AssetTransactionDto transaction);

        /// <summary>
        /// Проверяет статус подключения к Kafka
        /// </summary>
        /// <returns>Статус подключения</returns>
        Task<bool> IsConnectedAsync();
    }
}
