using Microsoft.EntityFrameworkCore;
using StockCardService.Abstractions.Repositories;
using StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.Repositories
{
    public abstract class Repository<T, TPrimaryKey> : IRepository<T, TPrimaryKey> where T
       : class, IEntity<TPrimaryKey>
    {
        protected readonly StockCardDbContext Context;
        private readonly DbSet<T> _entitySet;

        protected Repository(StockCardDbContext context)
        {
            Context = context;
            _entitySet = Context.Set<T>();
        }

        /// <summary>
        /// Запросить все сущности в базе.
        /// </summary>
        /// <param name="asNoTracking"> Вызвать с AsNoTracking. </param>
        /// <returns> IQueryable массив сущностей. </returns>
        public virtual IQueryable<T> GetAll(bool asNoTracking = false)
        {
            return asNoTracking ? _entitySet.AsNoTracking() : _entitySet;
        }

        /// <summary>
        /// Запросить все сущности в базе.
        /// </summary>
        /// <param name="cancellationToken"> Токен отмены </param>
        /// <param name="asNoTracking"> Вызвать с AsNoTracking. </param>
        /// <returns> Список сущностей. </returns>
        public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken, bool asNoTracking = false)
        {
            return await GetAll().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Получить сущность по ID.
        /// </summary>
        /// <param name="id"> Id сущности. </param>
        /// <returns> Cущность. </returns>
        public virtual T GetById(TPrimaryKey id)
        {
            return _entitySet.Find(id);
        }

        /// <summary>
        /// Получить сущность по Id.
        /// </summary>
        /// <param name="id"> Id сущности. </param>
        /// <param name="cancellationToken"></param>
        /// <returns> Cущность. </returns>
        public virtual async Task<T> GetByIdAsync(TPrimaryKey id, CancellationToken cancellationToken)
        {
            return await _entitySet.FindAsync((object)id);
        }

        /// <summary>
        /// Добавить в базу одну сущность.
        /// </summary>
        /// <param name="entity"> Сущность для добавления. </param>
        /// <returns> Добавленная сущность. </returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            return (await _entitySet.AddAsync(entity)).Entity;
        }

    }
}
