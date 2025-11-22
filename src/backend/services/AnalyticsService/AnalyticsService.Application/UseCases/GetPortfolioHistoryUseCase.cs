using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;

namespace StockMarketAssistant.AnalyticsService.Application.UseCases
{
    /// <summary>
    /// Use Case для получения истории портфеля
    /// </summary>
    public class GetPortfolioHistoryUseCase
    {
        private readonly IPortfolioServiceClient _portfolioServiceClient;
        private readonly ILogger<GetPortfolioHistoryUseCase> _logger;

        public GetPortfolioHistoryUseCase(
            IPortfolioServiceClient portfolioServiceClient,
            ILogger<GetPortfolioHistoryUseCase> logger)
        {
            _portfolioServiceClient = portfolioServiceClient ?? throw new ArgumentNullException(nameof(portfolioServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Выполнить use case
        /// </summary>
        public async Task<PortfolioHistoryDto?> ExecuteAsync(
            Guid portfolioId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            // Валидация дат
            if (startDate >= endDate)
            {
                throw new ArgumentException("Начальная дата должна быть меньше конечной даты", nameof(startDate));
            }

            _logger.LogInformation(
                "Получение истории портфеля {PortfolioId} за период {Start} - {End}",
                portfolioId, startDate, endDate);

            // Вызов IPortfolioServiceClient.GetHistoryAsync()
            var history = await _portfolioServiceClient.GetHistoryAsync(
                portfolioId,
                startDate,
                endDate,
                cancellationToken);

            if (history == null)
            {
                _logger.LogWarning("История портфеля {PortfolioId} не найдена", portfolioId);
            }
            else
            {
                _logger.LogInformation(
                    "Получена история портфеля {PortfolioId}: {TransactionCount} транзакций",
                    portfolioId, history.Transactions.Count);
            }

            return history;
        }
    }
}

