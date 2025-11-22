using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с рейтингами активов
    /// </summary>
    public class AssetRatingRepository : IAssetRatingRepository
    {
        private readonly AnalyticsDbContext _context;

        /// <summary>
        /// Конструктор
        /// </summary>
        public AssetRatingRepository(AnalyticsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<AssetRating?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AssetRatings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AssetRatings
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AssetRating?> GetByStockCardAndPeriodAsync(
            Guid stockCardId,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.StockCardId == stockCardId
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End
                         && r.Context == context);

            if (context == AnalysisContext.Portfolio && portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }
            else if (context == AnalysisContext.Global)
            {
                query = query.Where(r => r.PortfolioId == null);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetByPortfolioIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.PortfolioId == portfolioId)
                .OrderByDescending(r => r.PeriodEnd)
                .ThenByDescending(r => r.TransactionCountRank)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetByPortfolioAndPeriodAsync(
            Guid portfolioId,
            Period period,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.PortfolioId == portfolioId
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End)
                .OrderByDescending(r => r.TransactionCountRank)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetByContextAndPeriodAsync(
            AnalysisContext context,
            Period period,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.Context == context
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End);

            if (context == AnalysisContext.Portfolio && portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }
            else if (context == AnalysisContext.Global)
            {
                query = query.Where(r => r.PortfolioId == null);
            }

            return await query
                .OrderByDescending(r => r.TransactionCountRank)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetTopBoughtAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.Context == context
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End);

            if (context == AnalysisContext.Portfolio && portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }
            else if (context == AnalysisContext.Global)
            {
                query = query.Where(r => r.PortfolioId == null);
            }

            return await query
                .OrderByDescending(r => r.BuyTransactionCount)
                .ThenByDescending(r => r.TotalBuyAmount)
                .Take(topCount)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetTopSoldAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.Context == context
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End);

            if (context == AnalysisContext.Portfolio && portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }
            else if (context == AnalysisContext.Global)
            {
                query = query.Where(r => r.PortfolioId == null);
            }

            return await query
                .OrderByDescending(r => r.SellTransactionCount)
                .ThenByDescending(r => r.TotalSellAmount)
                .Take(topCount)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetTopByBuyAmountAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.Context == context
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End);

            if (context == AnalysisContext.Portfolio && portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }
            else if (context == AnalysisContext.Global)
            {
                query = query.Where(r => r.PortfolioId == null);
            }

            return await query
                .OrderByDescending(r => r.TotalBuyAmount)
                .Take(topCount)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetRating>> GetTopBySellAmountAsync(
            int topCount,
            Period period,
            AnalysisContext context,
            Guid? portfolioId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetRatings
                .AsNoTracking()
                .Where(r => r.Context == context
                         && r.PeriodStart == period.Start
                         && r.PeriodEnd == period.End);

            if (context == AnalysisContext.Portfolio && portfolioId.HasValue)
            {
                query = query.Where(r => r.PortfolioId == portfolioId);
            }
            else if (context == AnalysisContext.Global)
            {
                query = query.Where(r => r.PortfolioId == null);
            }

            return await query
                .OrderByDescending(r => r.TotalSellAmount)
                .Take(topCount)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(AssetRating rating, CancellationToken cancellationToken = default)
        {
            if (rating == null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            await _context.AssetRatings.AddAsync(rating, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddRangeAsync(IEnumerable<AssetRating> ratings, CancellationToken cancellationToken = default)
        {
            if (ratings == null)
            {
                throw new ArgumentNullException(nameof(ratings));
            }

            await _context.AssetRatings.AddRangeAsync(ratings, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(AssetRating rating, CancellationToken cancellationToken = default)
        {
            if (rating == null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            _context.AssetRatings.Update(rating);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task UpsertAsync(AssetRating rating, CancellationToken cancellationToken = default)
        {
            if (rating == null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            var existing = await GetByStockCardAndPeriodAsync(
                rating.StockCardId,
                new Period(rating.PeriodStart, rating.PeriodEnd),
                rating.Context,
                rating.PortfolioId,
                cancellationToken);

            if (existing != null)
            {
                // Обновляем существующий рейтинг
                _context.Entry(existing).CurrentValues.SetValues(rating);
                _context.AssetRatings.Update(existing);
            }
            else
            {
                // Добавляем новый рейтинг
                await _context.AssetRatings.AddAsync(rating, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task UpsertBatchAsync(IEnumerable<AssetRating> ratings, CancellationToken cancellationToken = default)
        {
            if (ratings == null)
            {
                throw new ArgumentNullException(nameof(ratings));
            }

            var ratingsList = ratings.ToList();
            if (ratingsList.Count == 0)
            {
                return;
            }

            // Оптимизированный batch upsert через EF Core
            // Для больших объемов данных можно использовать PostgreSQL ON CONFLICT через raw SQL
            // Но для большинства случаев текущая реализация достаточна
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var rating in ratingsList)
                {
                    // Проверяем существование по уникальному ключу
                    var existing = await _context.AssetRatings
                        .Where(r => r.StockCardId == rating.StockCardId
                                 && r.PeriodStart == rating.PeriodStart
                                 && r.PeriodEnd == rating.PeriodEnd
                                 && r.Context == rating.Context
                                 && r.PortfolioId == rating.PortfolioId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existing != null)
                    {
                        // Обновляем существующий рейтинг
                        _context.Entry(existing).CurrentValues.SetValues(rating);
                        _context.AssetRatings.Update(existing);
                    }
                    else
                    {
                        // Добавляем новый рейтинг
                        await _context.AssetRatings.AddAsync(rating, cancellationToken);
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        /// <inheritdoc />
        public Task DeleteAsync(AssetRating rating, CancellationToken cancellationToken = default)
        {
            if (rating == null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            _context.AssetRatings.Remove(rating);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var rating = await _context.AssetRatings.FindAsync(new object[] { id }, cancellationToken);
            if (rating != null)
            {
                _context.AssetRatings.Remove(rating);
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

