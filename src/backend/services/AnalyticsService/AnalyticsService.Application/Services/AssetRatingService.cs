using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;

namespace StockMarketAssistant.AnalyticsService.Application.Services
{
    /// <summary>
    /// Сервис для работы с рейтингом активов
    /// </summary>
    public class AssetRatingService : IAssetRatingService
    {
        private readonly IAssetRatingRepository _assetRatingRepository;
        private readonly IAssetTransactionRepository _transactionRepository;
        private readonly ILogger<AssetRatingService> _logger;

        public AssetRatingService(
            IAssetRatingRepository assetRatingRepository,
            IAssetTransactionRepository transactionRepository,
            ILogger<AssetRatingService> logger)
        {
            _assetRatingRepository = assetRatingRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<AssetRatingDto>> GetAssetRatingsAsync(AssetRatingRequestDto request)
        {
            _logger.LogInformation("Получение рейтинга активов для периода {PeriodStart} - {PeriodEnd}",
                request.PeriodStart, request.PeriodEnd);

            // Валидация параметров
            if (request.PeriodStart >= request.PeriodEnd)
            {
                throw new ArgumentException("Дата начала периода должна быть меньше даты окончания");
            }

            if (request.Limit <= 0 || request.Limit > 1000)
            {
                throw new ArgumentException("Количество записей должно быть от 1 до 1000");
            }

            // Определение контекста анализа
            var context = request.PortfolioId.HasValue ? AnalysisContext.Portfolio : AnalysisContext.Global;

            // Получение рейтингов в зависимости от типа сортировки
            IEnumerable<AssetRating> ratings;
            switch (request.SortBy)
            {
                case RatingSortType.TransactionCount:
                    ratings = await _assetRatingRepository.GetTopByTransactionCountAsync(
                        request.PeriodStart, request.PeriodEnd, request.Limit, context, request.PortfolioId);
                    break;
                case RatingSortType.TransactionAmount:
                    ratings = await _assetRatingRepository.GetTopByTransactionAmountAsync(
                        request.PeriodStart, request.PeriodEnd, request.Limit, context, request.PortfolioId);
                    break;
                default:
                    ratings = await _assetRatingRepository.GetTopByTransactionCountAsync(
                        request.PeriodStart, request.PeriodEnd, request.Limit, context, request.PortfolioId);
                    break;
            }

            // Применение направления сортировки
            if (request.SortDirection == SortDirection.Descending)
            {
                ratings = ratings.OrderByDescending(r => r.TransactionCountRank);
            }
            else
            {
                ratings = ratings.OrderBy(r => r.TransactionCountRank);
            }

            // Применение пагинации
            ratings = ratings.Skip(request.Offset).Take(request.Limit);

            // Преобразование в DTO
            return ratings.Select(MapToDto);
        }

        public async Task<IEnumerable<AssetRatingDto>> GetTopBuyingAssetsAsync(DateTime periodStart, DateTime periodEnd, int limit, Guid? portfolioId)
        {
            _logger.LogInformation("Получение топ {Limit} покупаемых активов для периода {PeriodStart} - {PeriodEnd}",
                limit, periodStart, periodEnd);

            var context = portfolioId.HasValue ? AnalysisContext.Portfolio : AnalysisContext.Global;
            var ratings = await _assetRatingRepository.GetTopByTransactionAmountAsync(periodStart, periodEnd, limit, context, portfolioId);

            return ratings
                .OrderByDescending(r => r.TotalBuyAmount)
                .Take(limit)
                .Select(MapToDto);
        }

        public async Task<IEnumerable<AssetRatingDto>> GetTopSellingAssetsAsync(DateTime periodStart, DateTime periodEnd, int limit, Guid? portfolioId)
        {
            _logger.LogInformation("Получение топ {Limit} продаваемых активов для периода {PeriodStart} - {PeriodEnd}",
                limit, periodStart, periodEnd);

            var context = portfolioId.HasValue ? AnalysisContext.Portfolio : AnalysisContext.Global;
            var ratings = await _assetRatingRepository.GetTopByTransactionAmountAsync(periodStart, periodEnd, limit, context, portfolioId);

            return ratings
                .OrderByDescending(r => r.TotalSellAmount)
                .Take(limit)
                .Select(MapToDto);
        }

        public async Task UpdateAssetRatingsAsync(AssetTransactionDto transaction)
        {
            _logger.LogInformation("Обновление рейтинга для актива {StockCardId} портфеля {PortfolioId}",
                transaction.StockCardId, transaction.PortfolioId);

            // Здесь должна быть логика обновления рейтингов
            // В реальной реализации это может включать:
            // 1. Сохранение транзакции
            // 2. Пересчет рейтингов для соответствующего периода
            // 3. Обновление рангов

            await Task.CompletedTask; // Заглушка для базовой реализации
        }

        public async Task RecalculateRatingsAsync(DateTime periodStart, DateTime periodEnd, Guid? portfolioId)
        {
            _logger.LogInformation("Пересчет рейтингов для периода {PeriodStart} - {PeriodEnd}",
                periodStart, periodEnd);

            // Здесь должна быть логика пересчета рейтингов
            // В реальной реализации это может включать:
            // 1. Получение всех транзакций за период
            // 2. Агрегация данных по активам
            // 3. Расчет рангов
            // 4. Сохранение обновленных рейтингов

            await Task.CompletedTask; // Заглушка для базовой реализации
        }

        private static AssetRatingDto MapToDto(AssetRating rating)
        {
            return new AssetRatingDto
            {
                StockCardId = rating.StockCardId,
                AssetType = rating.AssetType.ToString(),
                Ticker = rating.Ticker,
                Name = rating.Name,
                PeriodStart = rating.PeriodStart,
                PeriodEnd = rating.PeriodEnd,
                BuyTransactionCount = rating.BuyTransactionCount,
                SellTransactionCount = rating.SellTransactionCount,
                TotalBuyAmount = rating.TotalBuyAmount,
                TotalSellAmount = rating.TotalSellAmount,
                TotalBuyQuantity = rating.TotalBuyQuantity,
                TotalSellQuantity = rating.TotalSellQuantity,
                TransactionCountRank = rating.TransactionCountRank,
                TransactionAmountRank = rating.TransactionAmountRank,
                Context = rating.Context.ToString(),
                PortfolioId = rating.PortfolioId
            };
        }
    }
}
