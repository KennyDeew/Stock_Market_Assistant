using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence.Repositories
{
    /// <summary>
    /// Реализация репозитория для работы с транзакциями активов
    /// </summary>
    public class AssetTransactionRepository : IAssetTransactionRepository
    {
        private readonly AnalyticsDbContext _context;

        /// <summary>
        /// Конструктор
        /// </summary>
        public AssetTransactionRepository(AnalyticsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<AssetTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetByPortfolioIdAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .Where(t => t.PortfolioId == portfolioId)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetByStockCardIdAsync(
            Guid stockCardId,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .Where(t => t.StockCardId == stockCardId)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetByPeriodAsync(
            Period period,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .Where(t => t.TransactionTime >= period.Start && t.TransactionTime <= period.End)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetByPortfolioAndPeriodAsync(
            Guid portfolioId,
            Period period,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .Where(t => t.PortfolioId == portfolioId
                         && t.TransactionTime >= period.Start
                         && t.TransactionTime <= period.End)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetByStockCardAndPeriodAsync(
            Guid stockCardId,
            Period period,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .Where(t => t.StockCardId == stockCardId
                         && t.TransactionTime >= period.Start
                         && t.TransactionTime <= period.End)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AssetTransaction>> GetByTransactionTypeAsync(
            TransactionType transactionType,
            CancellationToken cancellationToken = default)
        {
            return await _context.AssetTransactions
                .AsNoTracking()
                .Where(t => t.TransactionType == transactionType)
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IGrouping<Guid, AssetTransaction>>> GetGroupedByStockCardAsync(
            Period? period = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AssetTransactions.AsNoTracking();

            if (period != null)
            {
                query = query.Where(t => t.TransactionTime >= period.Start && t.TransactionTime <= period.End);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionTime)
                .ToListAsync(cancellationToken);

            return transactions.GroupBy(t => t.StockCardId);
        }

        /// <inheritdoc />
        public async Task AddAsync(AssetTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            await _context.AssetTransactions.AddAsync(transaction, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddRangeAsync(IEnumerable<AssetTransaction> transactions, CancellationToken cancellationToken = default)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions));
            }

            await _context.AssetTransactions.AddRangeAsync(transactions, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(AssetTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            _context.AssetTransactions.Update(transaction);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(AssetTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            _context.AssetTransactions.Remove(transaction);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var transaction = await _context.AssetTransactions.FindAsync(new object[] { id }, cancellationToken);
            if (transaction != null)
            {
                _context.AssetTransactions.Remove(transaction);
            }
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

