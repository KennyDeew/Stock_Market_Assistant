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
            var transactionsList = transactions.ToList();
            _logger.LogInformation("Найдено транзакций для Portfolio рейтингов: {Count}", transactionsList.Count);

            var portfolioIds = transactionsList
                .Select(t => t.PortfolioId)
                .Distinct()
                .ToList();

            _logger.LogInformation("Найдено уникальных PortfolioId: {Count}", portfolioIds.Count);

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

            // Сначала проверяем, есть ли транзакции в периоде
            var transactionsInPeriod = await _transactionRepository.GetByPeriodAsync(period, cancellationToken);
            var transactionsInPeriodList = transactionsInPeriod.ToList();
            _logger.LogInformation("Транзакций в периоде (прямая проверка): {Count}", transactionsInPeriodList.Count);

            if (!transactionsInPeriodList.Any())
            {
                _logger.LogWarning("ВНИМАНИЕ: В периоде {Start} - {End} нет транзакций!", period.Start, period.End);
                return; // Прерываем выполнение, если нет транзакций
            }

            // Получаем транзакции за период, сгруппированные по StockCardId
            var transactionGroups = await _transactionRepository.GetGroupedByStockCardAsync(period, cancellationToken);
            var groupsList = transactionGroups.ToList();
            _logger.LogInformation("Найдено групп транзакций для Global рейтинга: {Count}", groupsList.Count);

            if (!groupsList.Any())
            {
                _logger.LogWarning("ВНИМАНИЕ: Не найдено групп транзакций для Global рейтинга!");
                return; // Прерываем выполнение, если нет групп
            }

            var ratings = new List<AssetRating>();

            foreach (var group in groupsList)
            {
                try
                {
                    var transactionsList = group.ToList();
                    _logger.LogInformation("Обработка группы StockCardId {StockCardId}, транзакций: {Count}", group.Key, transactionsList.Count);

                    // Получаем первую транзакцию для получения информации об активе
                    var firstTransaction = group.First();

                    // Используем значения по умолчанию для ticker и name, если они недоступны
                    // Тикер должен быть не длиннее 20 символов
                    var guidString = firstTransaction.StockCardId.ToString("N");
                    var ticker = $"STK{guidString.Substring(0, Math.Min(17, guidString.Length))}";
                    if (ticker.Length > 20)
                    {
                        ticker = ticker.Substring(0, 20);
                    }
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
                    _logger.LogInformation("Создан рейтинг для StockCardId {StockCardId}, BuyCount: {BuyCount}, SellCount: {SellCount}, TotalBuyAmount: {TotalBuyAmount}",
                        group.Key, rating.BuyTransactionCount, rating.SellTransactionCount, rating.TotalBuyAmount);
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
                    // Тикер должен быть не длиннее 20 символов
                    var guidString = firstTransaction.StockCardId.ToString("N");
                    var ticker = $"STK{guidString.Substring(0, Math.Min(17, guidString.Length))}";
                    if (ticker.Length > 20)
                    {
                        ticker = ticker.Substring(0, 20);
                    }
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

