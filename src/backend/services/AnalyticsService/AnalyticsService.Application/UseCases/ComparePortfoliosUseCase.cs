using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Constants;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Application.UseCases
{
    /// <summary>
    /// Use Case для сравнения портфелей
    /// </summary>
    public class ComparePortfoliosUseCase
    {
        private readonly IPortfolioServiceClient _portfolioServiceClient;
        private readonly IAssetTransactionRepository _transactionRepository;
        private readonly ILogger<ComparePortfoliosUseCase> _logger;

        public ComparePortfoliosUseCase(
            IPortfolioServiceClient portfolioServiceClient,
            IAssetTransactionRepository transactionRepository,
            ILogger<ComparePortfoliosUseCase> logger)
        {
            _portfolioServiceClient = portfolioServiceClient ?? throw new ArgumentNullException(nameof(portfolioServiceClient));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Результат сравнения портфелей
        /// </summary>
        public class ComparisonResult
        {
            public Dictionary<Guid, PortfolioStateDto> PortfolioStates { get; set; } = new();
            public Dictionary<Guid, List<PortfolioTransactionDto>> PortfolioTransactions { get; set; } = new();
        }

        /// <summary>
        /// Выполнить use case
        /// </summary>
        public async Task<ComparisonResult> ExecuteAsync(
            IEnumerable<Guid> portfolioIds,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            var portfolioIdsList = portfolioIds.ToList();

            // Валидация: макс 10 портфелей
            if (portfolioIdsList.Count > DomainConstants.Validation.MaxPortfoliosInComparison)
            {
                throw new ArgumentException(
                    $"Максимальное количество портфелей для сравнения: {DomainConstants.Validation.MaxPortfoliosInComparison}",
                    nameof(portfolioIds));
            }

            if (portfolioIdsList.Count == 0)
            {
                throw new ArgumentException("Необходимо указать хотя бы один портфель для сравнения", nameof(portfolioIds));
            }

            _logger.LogInformation("Сравнение {Count} портфелей", portfolioIdsList.Count);

            var result = new ComparisonResult();

            // Получение данных от PortfolioService
            var portfolioStates = await _portfolioServiceClient.GetMultipleStatesAsync(portfolioIdsList, cancellationToken);
            result.PortfolioStates = portfolioStates;

            _logger.LogInformation("Получено состояний портфелей: {Count}", portfolioStates.Count);

            // Получение транзакций из репозиториев
            Period? period = null;
            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value >= endDate.Value)
                {
                    throw new ArgumentException("Начальная дата должна быть меньше конечной даты", nameof(startDate));
                }
                period = Period.Custom(startDate.Value, endDate.Value);
            }

            foreach (var portfolioId in portfolioIdsList)
            {
                try
                {
                    IEnumerable<Domain.Entities.AssetTransaction> transactions;

                    if (period != null)
                    {
                        transactions = await _transactionRepository.GetByPortfolioAndPeriodAsync(
                            portfolioId,
                            period,
                            cancellationToken);
                    }
                    else
                    {
                        transactions = await _transactionRepository.GetByPortfolioIdAsync(portfolioId, cancellationToken);
                    }

                    var transactionDtos = transactions.Select(t => new PortfolioTransactionDto
                    {
                        Id = t.Id,
                        PortfolioAssetId = t.StockCardId, // Используем StockCardId как PortfolioAssetId для совместимости
                        StockCardId = t.StockCardId,
                        TransactionType = (int)t.TransactionType,
                        Quantity = t.Quantity,
                        PricePerUnit = t.PricePerUnit,
                        TotalAmount = t.TotalAmount,
                        TransactionDate = t.TransactionTime,
                        Currency = t.Currency
                    }).ToList();

                    result.PortfolioTransactions[portfolioId] = transactionDtos;

                    _logger.LogInformation(
                        "Получено {Count} транзакций для портфеля {PortfolioId}",
                        transactionDtos.Count, portfolioId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении транзакций для портфеля {PortfolioId}", portfolioId);
                    result.PortfolioTransactions[portfolioId] = new List<PortfolioTransactionDto>();
                }
            }

            _logger.LogInformation("Сравнение портфелей завершено");

            return result;
        }
    }
}

