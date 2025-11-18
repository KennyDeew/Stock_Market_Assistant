using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий EF Core для финансовых активов портфелей ценных бумаг
    /// </summary>
    /// <param name="dataContext"></param>
    public class PortfolioAssetRepository: EfRepository<PortfolioAsset, Guid>, IPortfolioAssetRepository
    {
        private readonly DbSet<PortfolioAssetTransaction> _transactions;

        public PortfolioAssetRepository(DatabaseContext dataContext)
            : base(dataContext)
        {
            _transactions = DataContext.Set<PortfolioAssetTransaction>();
        }

        public async Task<PortfolioAssetTransaction?> GetAssetTransactionByIdAsync(Guid transactionId)
        {
            return await _transactions.FindAsync(transactionId);
        }

        public async Task<IEnumerable<PortfolioAssetTransaction>> GetAssetTransactionsByAssetIdAsync(Guid assetId)
        {
            return await _transactions
                .Where(t => t.PortfolioAssetId == assetId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PortfolioAssetTransaction>> GetAssetTransactionsByAssetIdAndPeriodAsync(
            Guid assetId, DateTime startDate, DateTime endDate)
        {
            return await _transactions
                .Where(t => t.PortfolioAssetId == assetId &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<PortfolioAssetTransaction> AddAssetTransactionAsync(PortfolioAssetTransaction transaction)
        {
            var entityEntryAdded = await _transactions.AddAsync(transaction);
            await DataContext.SaveChangesAsync();
            return entityEntryAdded.Entity;
        }

        public async Task AddAssetTransactionsAsync(IEnumerable<PortfolioAssetTransaction> transactions)
        {
            await _transactions.AddRangeAsync(transactions);
            await DataContext.SaveChangesAsync();
        }

        public async Task UpdateAssetTransactionAsync(PortfolioAssetTransaction transaction)
        {
            _transactions.Update(transaction);
            await DataContext.SaveChangesAsync();
        }

        public async Task DeleteAssetTransactionAsync(Guid transactionId)
        {
            var transaction = await GetAssetTransactionByIdAsync(transactionId);
            if (transaction != null)
            {
                _transactions.Remove(transaction);
                await DataContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetAssetTransactionsCountAsync(Guid assetId)
        {
            return await _transactions
                .Where(t => t.PortfolioAssetId == assetId)
                .CountAsync();
        }

        public async Task<PortfolioAsset?> GetByPortfolioAndStockCardAsync(Guid portfolioId, Guid stockCardId, bool includeTransactions = false)
        {
            IQueryable<PortfolioAsset> query = Data;
            if (includeTransactions)
            {
                query = query.Include(a => a.Transactions);
            }
            return await query.FirstOrDefaultAsync(a => a.PortfolioId == portfolioId && a.StockCardId == stockCardId);
        }
    }
}
