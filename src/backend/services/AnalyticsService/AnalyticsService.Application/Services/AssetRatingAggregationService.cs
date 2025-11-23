using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Services;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Application.Services
{
    /// <summary>
    /// Сервис для оркестрации агрегации рейтингов активов
    /// </summary>
    public class AssetRatingAggregationService
    {
        private readonly IAssetTransactionRepository _transactionRepository;
        private readonly IAssetRatingRepository _ratingRepository;
        private readonly RatingCalculationService _ratingCalculationService;
        private readonly ILogger<AssetRatingAggregationService> _logger;

        public AssetRatingAggregationService(
            IAssetTransactionRepository transactionRepository,
            IAssetRatingRepository ratingRepository,
            RatingCalculationService ratingCalculationService,
            ILogger<AssetRatingAggregationService> logger)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _ratingCalculationService = ratingCalculationService ?? throw new ArgumentNullException(nameof(ratingCalculationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Агрегировать рейтинги за период для всех контекстов
        /// </summary>
        public async Task AggregateRatingsAsync(Period period, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Начало агрегации рейтингов за период {Start} - {End}", period.Start, period.End);

            // Агрегация для Global контекста
            await AggregateGlobalRatingsAsync(period, cancellationToken);

            // Агрегация для Portfolio контекста
            // Получаем все уникальные PortfolioId из транзакций
            var transactions = await _transactionRepository.GetByPeriodAsync(period, cancellationToken);
            var portfolioIds = transactions
                .Select(t => t.PortfolioId)
                .Distinct()
                .ToList();

            foreach (var portfolioId in portfolioIds)
            {
                await AggregatePortfolioRatingsAsync(portfolioId, period, cancellationToken);
            }

            _logger.LogInformation("Завершение агрегации рейтингов за период {Start} - {End}", period.Start, period.End);
        }

        /// <summary>
        /// Агрегировать рейтинги для Global контекста
        /// </summary>
        public async Task AggregateGlobalRatingsAsync(Period period, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Агрегация Global рейтингов за период {Start} - {End}", period.Start, period.End);

            // Получаем транзакции за период, сгруппированные по StockCardId
            var transactionGroups = await _transactionRepository.GetGroupedByStockCardAsync(period, cancellationToken);

            var ratings = new List<AssetRating>();

            foreach (var group in transactionGroups)
            {
                try
                {
                    // Получаем первую транзакцию для получения информации об активе
                    var firstTransaction = group.First();

                    // Используем значения по умолчанию для ticker и name, если они недоступны
                    var ticker = $"STOCK_{firstTransaction.StockCardId:N}";
                    var name = $"Asset {firstTransaction.StockCardId}";

                    // Создаем рейтинг с помощью RatingCalculationService
                    var rating = _ratingCalculationService.CreateRating(
                        group,
                        period,
                        AnalysisContext.Global,
                        null, // PortfolioId null для Global контекста
                        firstTransaction.AssetType,
                        ticker,
                        name);

                    ratings.Add(rating);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании рейтинга для StockCardId {StockCardId}", group.Key);
                }
            }

            // Назначаем ранги
            _ratingCalculationService.AssignRanks(ratings);

            // Сохраняем рейтинги
            await _ratingRepository.UpsertBatchAsync(ratings, cancellationToken);
            await _ratingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Агрегация Global рейтингов завершена. Создано {Count} рейтингов", ratings.Count);
        }

        /// <summary>
        /// Агрегировать рейтинги для Portfolio контекста
        /// </summary>
        public async Task AggregatePortfolioRatingsAsync(
            Guid portfolioId,
            Period period,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Агрегация Portfolio рейтингов для PortfolioId {PortfolioId} за период {Start} - {End}",
                portfolioId, period.Start, period.End);

            // Получаем транзакции портфеля за период, сгруппированные по StockCardId
            var transactions = await _transactionRepository.GetByPortfolioAndPeriodAsync(portfolioId, period, cancellationToken);
            var transactionGroups = transactions.GroupBy(t => t.StockCardId);

            var ratings = new List<AssetRating>();

            foreach (var group in transactionGroups)
            {
                try
                {
                    // Получаем первую транзакцию для получения информации об активе
                    var firstTransaction = group.First();

                    // Используем значения по умолчанию для ticker и name, если они недоступны
                    var ticker = $"STOCK_{firstTransaction.StockCardId:N}";
                    var name = $"Asset {firstTransaction.StockCardId}";

                    // Создаем рейтинг с помощью RatingCalculationService
                    var rating = _ratingCalculationService.CreateRating(
                        group,
                        period,
                        AnalysisContext.Portfolio,
                        portfolioId,
                        firstTransaction.AssetType,
                        ticker,
                        name);

                    ratings.Add(rating);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при создании рейтинга для PortfolioId {PortfolioId}, StockCardId {StockCardId}",
                        portfolioId, group.Key);
                }
            }

            // Назначаем ранги
            _ratingCalculationService.AssignRanks(ratings);

            // Сохраняем рейтинги
            await _ratingRepository.UpsertBatchAsync(ratings, cancellationToken);
            await _ratingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Агрегация Portfolio рейтингов для PortfolioId {PortfolioId} завершена. Создано {Count} рейтингов",
                portfolioId, ratings.Count);
        }
    }
}

