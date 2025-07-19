using StockMarketAssistant.PortfolioService.Domain.Entities;
using System.Linq.Expressions;

namespace StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories
{
    public interface IRepository<TEntity, TPrimaryKey>
        where TEntity : BaseEntity<TPrimaryKey>
        where TPrimaryKey : notnull
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity?> GetByIdAsync(TPrimaryKey id, params Expression<Func<TEntity, object>>[] includeProperties);

        Task<bool> ExistsAsync(TPrimaryKey id);

        Task<IEnumerable<TEntity>> GetRangeByIdsAsync(IList<TPrimaryKey> ids);

        Task<TEntity> AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    }

}
