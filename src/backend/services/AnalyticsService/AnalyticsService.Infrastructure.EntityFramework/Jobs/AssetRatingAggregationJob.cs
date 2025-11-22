using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Services;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Jobs
{
    /// <summary>
    /// Периодическая задача для пересчета рангов рейтингов активов
    /// Запускается каждые 15 минут
    /// </summary>
    public class AssetRatingAggregationJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AssetRatingAggregationJob> _logger;
        private readonly RatingCalculationService _ratingCalculationService;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

        public AssetRatingAggregationJob(
            IServiceProvider serviceProvider,
            ILogger<AssetRatingAggregationJob> logger,
            RatingCalculationService ratingCalculationService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _ratingCalculationService = ratingCalculationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AssetRatingAggregationJob запущен. Интервал: {Interval} минут", _interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RecalculateRanksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении AssetRatingAggregationJob");
                }

                // Ожидание до следующего запуска
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("AssetRatingAggregationJob остановлен");
        }

        /// <summary>
        /// Пересчет рангов для всех рейтингов
        /// </summary>
        private async Task RecalculateRanksAsync(CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Начало пересчета рангов рейтингов");

            using var scope = _serviceProvider.CreateScope();
            var ratingRepository = scope.ServiceProvider.GetRequiredService<IAssetRatingRepository>();

            // Определяем период для агрегации (последние 30 дней)
            var period = Period.LastMonth(DateTime.UtcNow);

            // Пересчитываем ранги для Global контекста
            await RecalculateRanksForContextAsync(
                ratingRepository,
                period,
                AnalysisContext.Global,
                portfolioId: null,
                cancellationToken);

            // Пересчитываем ранги для всех Portfolio контекстов
            // Получаем все уникальные PortfolioId из рейтингов
            var allRatings = await ratingRepository.GetByContextAndPeriodAsync(
                AnalysisContext.Portfolio,
                period,
                portfolioId: null,
                cancellationToken);

            var portfolioIds = allRatings
                .Where(r => r.PortfolioId.HasValue)
                .Select(r => r.PortfolioId!.Value)
                .Distinct()
                .ToList();

            foreach (var portfolioId in portfolioIds)
            {
                await RecalculateRanksForContextAsync(
                    ratingRepository,
                    period,
                    AnalysisContext.Portfolio,
                    portfolioId,
                    cancellationToken);
            }

            var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation(
                "Пересчет рангов завершен за {ProcessingTime}ms. Обработано портфелей: {PortfolioCount}",
                processingTime,
                portfolioIds.Count);
        }

        /// <summary>
        /// Пересчет рангов для конкретного контекста
        /// </summary>
        private async Task RecalculateRanksForContextAsync(
            IAssetRatingRepository ratingRepository,
            Period period,
            AnalysisContext context,
            Guid? portfolioId,
            CancellationToken cancellationToken)
        {
            // Получаем все рейтинги для контекста и периода
            var ratings = await ratingRepository.GetByContextAndPeriodAsync(
                context,
                period,
                portfolioId,
                cancellationToken);

            var ratingsList = ratings.ToList();

            if (ratingsList.Count == 0)
            {
                _logger.LogInformation(
                    "Нет рейтингов для пересчета: Context={Context}, PortfolioId={PortfolioId}",
                    context,
                    portfolioId);
                return;
            }

            // Пересчитываем ранги используя RatingCalculationService
            _ratingCalculationService.AssignRanks(ratingsList);

            // Сохраняем обновленные рейтинги
            foreach (var rating in ratingsList)
            {
                await ratingRepository.UpdateAsync(rating, cancellationToken);
            }

            await ratingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Ранги пересчитаны: Context={Context}, PortfolioId={PortfolioId}, Count={Count}",
                context,
                portfolioId,
                ratingsList.Count);
        }
    }
}

