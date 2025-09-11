using StockCardService.Domain.Entities;
using System.Linq.Expressions;

namespace StockCardService.Abstractions.Repositories
{
    /// <summary>
    /// Описания общих методов для MongoDb репозитория.
    /// </summary>
    /// <typeparam name="T"> Тип сущности для репозитория </typeparam>
    /// <typeparam name="TPrimaryKey"> Тип идентификатора сущности </typeparam>
    public interface IMongoRepository<T, TPrimaryKey>
        where T : IEntity<TPrimaryKey>
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIdAsync(TPrimaryKey id);

        Task<IEnumerable<T>> GetRangeByIdsAsync(List<TPrimaryKey> ids);

        Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(TPrimaryKey id);
    }
}
