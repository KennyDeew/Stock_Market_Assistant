using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий для работы с транзакциями активов
    /// </summary>
    public class AssetTransactionRepository : IAssetTransactionRepository
    {
        private readonly AnalyticsDbContext _context;
        private readonly ILogger<AssetTransactionRepository> _logger;

        public AssetTransactionRepository(AnalyticsDbContext context, ILogger<AssetTransactionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AssetTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.AssetTransactions.FindAsync(id);
        }

        public async Task<IEnumerable<AssetTransaction>> GetByParametersAsync(
            Guid? portfolioId = null,
            Guid? stockCardId = null,
            DateTime? periodStart = null,
            DateTime? periodEnd = null,
            AssetType? assetType = null,
            TransactionType? transactionType = null)
        {
            var query = _context.AssetTransactions.AsQueryable();

            if (portfolioId.HasValue)
                query = query.Where(t => t.PortfolioId == portfolioId.Value);

            if (stockCardId.HasValue)
                query = query.Where(t => t.StockCardId == stockCardId.Value);

            if (periodStart.HasValue)
                query = query.Where(t => t.TransactionTime >= periodStart.Value);

            if (periodEnd.HasValue)
                query = query.Where(t => t.TransactionTime <= periodEnd.Value);

            if (assetType.HasValue)
                query = query.Where(t => t.AssetType == assetType.Value);

            if (transactionType.HasValue)
                query = query.Where(t => t.TransactionType == transactionType.Value);

            return await query
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync();
        }

        public async Task<TransactionAggregation> GetAggregatedDataAsync(
            Guid? portfolioId,
            DateTime periodStart,
            DateTime periodEnd,
            AssetType? assetType = null)
        {
            var query = _context.AssetTransactions
                .Where(t => t.TransactionTime >= periodStart && t.TransactionTime <= periodEnd);

            if (portfolioId.HasValue)
                query = query.Where(t => t.PortfolioId == portfolioId.Value);

            if (assetType.HasValue)
                query = query.Where(t => t.AssetType == assetType.Value);

            var buyTransactions = query.Where(t => t.TransactionType == TransactionType.Buy);
            var sellTransactions = query.Where(t => t.TransactionType == TransactionType.Sell);

            var buyAggregation = await buyTransactions
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.TotalAmount),
                    TotalQuantity = g.Sum(t => t.Quantity)
                })
                .FirstOrDefaultAsync();

            var sellAggregation = await sellTransactions
                .GroupBy(t => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.TotalAmount),
                    TotalQuantity = g.Sum(t => t.Quantity)
                })
                .FirstOrDefaultAsync();

            return new TransactionAggregation
            {
                TotalTransactionCount = (buyAggregation?.Count ?? 0) + (sellAggregation?.Count ?? 0),
                TotalTransactionAmount = (buyAggregation?.TotalAmount ?? 0) + (sellAggregation?.TotalAmount ?? 0),
                BuyTransactionCount = buyAggregation?.Count ?? 0,
                SellTransactionCount = sellAggregation?.Count ?? 0,
                TotalBuyAmount = buyAggregation?.TotalAmount ?? 0,
                TotalSellAmount = sellAggregation?.TotalAmount ?? 0
            };
        }

        public async Task AddAsync(AssetTransaction transaction)
        {
            await _context.AssetTransactions.AddAsync(transaction);
            _logger.LogInformation("Добавлена новая транзакция {Id} для актива {StockCardId}",
                transaction.Id, transaction.StockCardId);
        }

        public async Task UpdateAsync(AssetTransaction transaction)
        {
            _context.AssetTransactions.Update(transaction);
            _logger.LogInformation("Обновлена транзакция {Id} для актива {StockCardId}",
                transaction.Id, transaction.StockCardId);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var transaction = await GetByIdAsync(id);
            if (transaction != null)
            {
                _context.AssetTransactions.Remove(transaction);
                _logger.LogInformation("Удалена транзакция {Id}", id);
            }
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
