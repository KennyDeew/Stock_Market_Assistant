using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Constants;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Application.UseCases
{
    /// <summary>
    /// Use Case для получения топ активов по количеству покупок
    /// </summary>
    public class GetTopBoughtAssetsUseCase
    {
        private readonly IAssetRatingRepository _ratingRepository;
        private readonly ILogger<GetTopBoughtAssetsUseCase> _logger;

        public GetTopBoughtAssetsUseCase(
            IAssetRatingRepository ratingRepository,
            ILogger<GetTopBoughtAssetsUseCase> logger)
        {
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Выполнить use case
        /// </summary>
        public async Task<IEnumerable<AssetRating>> ExecuteAsync(
            DateTime startDate,
            DateTime endDate,
            int top = DomainConstants.Aggregation.DefaultTopAssetsCount,
            AnalysisContext context = AnalysisContext.Global,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            // Валидация входных данных
            if (startDate >= endDate)
            {
                throw new ArgumentException("Начальная дата должна быть меньше конечной даты", nameof(startDate));
            }

            if (top < 1 || top > DomainConstants.Aggregation.MaxTopAssetsCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(top),
                    top,
                    $"Количество топ активов должно быть между 1 и {DomainConstants.Aggregation.MaxTopAssetsCount}");
            }

            // Создание Period
            var period = Period.Custom(startDate, endDate);

            _logger.LogInformation(
                "Получение топ {Top} активов по покупкам за период {Start} - {End}, Context: {Context}, PortfolioId: {PortfolioId}",
                top, startDate, endDate, context, portfolioId);

            // Вызов GetTopBoughtAsync
            var topBoughtAssets = await _ratingRepository.GetTopBoughtAsync(
                top,
                period,
                context,
                portfolioId,
                cancellationToken);

            _logger.LogInformation("Получено {Count} топ активов по покупкам", topBoughtAssets.Count());

            return topBoughtAssets;
        }
    }
}

