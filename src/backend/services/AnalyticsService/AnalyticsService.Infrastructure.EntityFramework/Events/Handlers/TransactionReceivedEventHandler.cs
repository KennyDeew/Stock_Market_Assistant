using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.Events;
using StockMarketAssistant.AnalyticsService.Domain.Services;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Events.Handlers
{
    /// <summary>
    /// Обработчик события получения транзакции
    /// Обновляет рейтинги активов инкрементально для Global и Portfolio контекстов
    /// </summary>
    public class TransactionReceivedEventHandler : IEventHandler<TransactionReceivedEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TransactionReceivedEventHandler> _logger;
        private readonly RatingCalculationService _ratingCalculationService;

        public TransactionReceivedEventHandler(
            IServiceProvider serviceProvider,
            ILogger<TransactionReceivedEventHandler> logger,
            RatingCalculationService ratingCalculationService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _ratingCalculationService = ratingCalculationService;
        }

        /// <summary>
        /// Обработка события получения транзакции
        /// </summary>
        public async Task HandleAsync(TransactionReceivedEvent @event, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation(
                "Обработка события TransactionReceivedEvent: TransactionId={TransactionId}, PortfolioId={PortfolioId}, StockCardId={StockCardId}",
                @event.Transaction.Id,
                @event.Transaction.PortfolioId,
                @event.Transaction.StockCardId);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var ratingRepository = scope.ServiceProvider.GetRequiredService<IAssetRatingRepository>();

                // Определяем период для агрегации (последние 30 дней)
                var period = Period.LastMonth(DateTime.UtcNow);

                // Обновляем Global контекст
                await UpdateRatingAsync(
                    ratingRepository,
                    @event.Transaction,
                    period,
                    AnalysisContext.Global,
                    portfolioId: null,
                    cancellationToken);

                // Обновляем Portfolio контекст
                await UpdateRatingAsync(
                    ratingRepository,
                    @event.Transaction,
                    period,
                    AnalysisContext.Portfolio,
                    portfolioId: @event.Transaction.PortfolioId,
                    cancellationToken);

                var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation(
                    "Событие TransactionReceivedEvent обработано за {ProcessingTime}ms",
                    processingTime);

                if (processingTime > 2000)
                {
                    _logger.LogWarning(
                        "Обработка события превысила SLA (2 секунды): {ProcessingTime}ms",
                        processingTime);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при обработке события TransactionReceivedEvent: TransactionId={TransactionId}",
                    @event.Transaction.Id);
                throw;
            }
        }

        /// <summary>
        /// Обновление рейтинга для указанного контекста
        /// </summary>
        private async Task UpdateRatingAsync(
            IAssetRatingRepository ratingRepository,
            AssetTransaction transaction,
            Period period,
            AnalysisContext context,
            Guid? portfolioId,
            CancellationToken cancellationToken)
        {
            // Получаем существующий рейтинг или создаем новый
            var existingRating = await ratingRepository.GetByStockCardAndPeriodAsync(
                transaction.StockCardId,
                period,
                context,
                portfolioId,
                cancellationToken);

            AssetRating updatedRating;

            if (existingRating != null)
            {
                // Инкрементальное обновление существующего рейтинга
                updatedRating = _ratingCalculationService.CalculateIncrementalUpdate(
                    existingRating,
                    transaction);
            }
            else
            {
                // Создаем новый рейтинг
                // Для нового рейтинга нужна информация об активе (ticker, name)
                // Используем значения по умолчанию, если информация недоступна
                var ticker = $"STOCK_{transaction.StockCardId:N}";
                var name = $"Asset {transaction.StockCardId}";

                if (context == AnalysisContext.Portfolio)
                {
                    updatedRating = AssetRating.CreatePortfolioRating(
                        portfolioId: portfolioId!.Value,
                        stockCardId: transaction.StockCardId,
                        assetType: transaction.AssetType,
                        ticker: ticker,
                        name: name,
                        periodStart: period.Start,
                        periodEnd: period.End);
                }
                else
                {
                    updatedRating = AssetRating.CreateGlobalRating(
                        stockCardId: transaction.StockCardId,
                        assetType: transaction.AssetType,
                        ticker: ticker,
                        name: name,
                        periodStart: period.Start,
                        periodEnd: period.End);
                }

                // Инициализируем счетчики на основе первой транзакции
                if (transaction.TransactionType == TransactionType.Buy)
                {
                    updatedRating.UpdateStatistics(
                        buyTransactionCount: 1,
                        sellTransactionCount: 0,
                        totalBuyAmount: transaction.TotalAmount,
                        totalSellAmount: 0,
                        totalBuyQuantity: transaction.Quantity,
                        totalSellQuantity: 0,
                        transactionCountRank: 0,
                        transactionAmountRank: 0);
                }
                else
                {
                    updatedRating.UpdateStatistics(
                        buyTransactionCount: 0,
                        sellTransactionCount: 1,
                        totalBuyAmount: 0,
                        totalSellAmount: transaction.TotalAmount,
                        totalBuyQuantity: 0,
                        totalSellQuantity: transaction.Quantity,
                        transactionCountRank: 0,
                        transactionAmountRank: 0);
                }
            }

            // Сохраняем обновленный рейтинг
            if (existingRating != null)
            {
                await ratingRepository.UpdateAsync(updatedRating, cancellationToken);
            }
            else
            {
                await ratingRepository.AddAsync(updatedRating, cancellationToken);
            }

            await ratingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Рейтинг обновлен: StockCardId={StockCardId}, Context={Context}, PortfolioId={PortfolioId}",
                transaction.StockCardId,
                context,
                portfolioId);
        }
    }
}

