using Microsoft.EntityFrameworkCore;
using StockCardService.Abstractions.Repositories;
using StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;
using System.Linq.Expressions;

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
        /// Получить сущность по Id со связанными объектами.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="includeProperties">массив делегатов для связанных объектов</param>
        /// <returns></returns>
        public virtual async Task<T> GetByIdWithLinkedItemsAsync(TPrimaryKey id, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _entitySet;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            var entity = await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
            return entity;
        }

        /// <summary>
        /// Добавить в базу одну сущность.
        /// </summary>
        /// <param name="entity"> Сущность для добавления. </param>
        /// <returns> Добавленная сущность. </returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            var createdEntity = await _entitySet.AddAsync(entity);
            await Context.SaveChangesAsync();
            return createdEntity.Entity;
        }

        /// <summary>
        /// Обновить в базе одну сущность.
        /// </summary>
        /// <param name="entity"> Обновленная сущность</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task UpdateAsync(T entity)
        {
            Context.Update(entity);
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Удалить в базе одну сущность.
        /// </summary>
        /// <param name="id"> Id удаляемой сущности. </param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DeleteAsync(TPrimaryKey id)
        {
            var entity = await GetByIdAsync(id, CancellationToken.None);
            if (entity == null)
                return;
            Context.Remove(entity);
            await Context.SaveChangesAsync();
        }
    }
}
