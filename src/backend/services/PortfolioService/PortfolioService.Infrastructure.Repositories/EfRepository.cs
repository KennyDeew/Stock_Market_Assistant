using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Domain.Entities;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using System.Linq.Expressions;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Repositories
{
    /// <summary>
    /// Репозиторий EF Core
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности</typeparam>
    /// <typeparam name="TPrimaryKey">Тип первичного ключа</typeparam>
    public class EfRepository<TEntity, TPrimaryKey>
        : IRepository<TEntity, TPrimaryKey>
        where TEntity : BaseEntity<TPrimaryKey>
        where TPrimaryKey : notnull
    {
        private readonly DatabaseContext _dataContext;

        protected DbSet<TEntity> Data { get; set; }

        public EfRepository(DatabaseContext dataContext)
        {
            _dataContext = dataContext;
            Data = _dataContext.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(TPrimaryKey id, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = Data;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            var entity = await query.FirstOrDefaultAsync(x => x.Id.Equals(id));
            return entity;
        }

        public async Task<bool> ExistsAsync(TPrimaryKey id)
        {
            TEntity? entity = await GetByIdAsync(id);
            return entity is not null;
        }

        public async Task<IEnumerable<TEntity>> GetRangeByIdsAsync(IList<TPrimaryKey> ids)
        {
            var entities = await Data.Where(x => ids.Contains(x.Id)).ToListAsync();
            return entities;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var entities = await Data.ToListAsync();
            return entities;
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var entityEntryAdded = await Data.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
            return entityEntryAdded.Entity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            Data.Update(entity);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            Data.Remove(entity);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Data.FirstOrDefaultAsync(predicate);
        }
    }
}
