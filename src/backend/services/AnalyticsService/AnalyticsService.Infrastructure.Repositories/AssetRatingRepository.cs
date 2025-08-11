using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с рейтингом активов
    /// </summary>
    public class AssetRatingRepository : IAssetRatingRepository
    {
        private readonly AnalyticsDbContext _context;
        private readonly ILogger<AssetRatingRepository> _logger;

        public AssetRatingRepository(AnalyticsDbContext context, ILogger<AssetRatingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AssetRating?> GetByIdAsync(Guid id)
        {
            return await _context.AssetRatings.FindAsync(id);
        }

        public async Task<IEnumerable<AssetRating>> GetByParametersAsync(Guid stockCardId, DateTime periodStart, DateTime periodEnd, AnalysisContext context, Guid? portfolioId = null)
        {
            var query = _context.AssetRatings
                .Where(r => r.StockCardId == stockCardId &&
                           r.PeriodStart == periodStart &&
                           r.PeriodEnd == periodEnd &&
                           r.Context == context);

            if (portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<AssetRating>> GetTopByTransactionCountAsync(DateTime periodStart, DateTime periodEnd, int limit, AnalysisContext context, Guid? portfolioId = null)
        {
            var query = _context.AssetRatings
                .Where(r => r.PeriodStart == periodStart &&
                           r.PeriodEnd == periodEnd &&
                           r.Context == context);

            if (portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }

            return await query
                .OrderByDescending(r => r.BuyTransactionCount + r.SellTransactionCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetRating>> GetTopByTransactionAmountAsync(DateTime periodStart, DateTime periodEnd, int limit, AnalysisContext context, Guid? portfolioId = null)
        {
            var query = _context.AssetRatings
                .Where(r => r.PeriodStart == periodStart &&
                           r.PeriodEnd == periodEnd &&
                           r.Context == context);

            if (portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }

            return await query
                .OrderByDescending(r => r.TotalBuyAmount + r.TotalSellAmount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddAsync(AssetRating assetRating)
        {
            await _context.AssetRatings.AddAsync(assetRating);
            _logger.LogInformation("Добавлен новый рейтинг для актива {StockCardId}", assetRating.StockCardId);
        }

        public async Task UpdateAsync(AssetRating assetRating)
        {
            _context.AssetRatings.Update(assetRating);
            _logger.LogInformation("Обновлен рейтинг для актива {StockCardId}", assetRating.StockCardId);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var rating = await GetByIdAsync(id);
            if (rating != null)
            {
                _context.AssetRatings.Remove(rating);
                _logger.LogInformation("Удален рейтинг {Id}", id);
            }
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
