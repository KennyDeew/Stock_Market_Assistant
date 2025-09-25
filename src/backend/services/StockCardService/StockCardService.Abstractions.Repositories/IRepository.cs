using StockCardService.Domain.Entities;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace StockCardService.Abstractions.Repositories
{
    /// <summary>
    /// Описания общих методов для всех репозиториев.
    /// </summary>
    /// <typeparam name="T"> Тип сущности для репозитория </typeparam>
    /// <typeparam name="TPrimaryKey"> Тип идентификатора сущности </typeparam>
    public interface IRepository<T, TPrimaryKey>
        where T : IEntity<TPrimaryKey>
    {
        /// <summary>
        /// Запросить все сущности в базе.
        /// </summary>
        /// <param name="noTracking"> Вызвать с AsNoTracking.</param>
        /// <returns> IQueryable массив сущностей.</returns>
        IQueryable<T> GetAll(bool noTracking = false);

        /// <summary>
        /// Запросить все сущности в базе.
        /// </summary>
        /// <param name="cancellationToken"> Токен отмены. </param>
        /// <param name="asNoTracking"> Вызвать с AsNoTracking. </param>
        /// <returns> Список сущностей. </returns>
        Task<List<T>> GetAllAsync(CancellationToken cancellationToken, bool asNoTracking = false);

        /// <summary>
        /// Получить сущность по Id.
        /// </summary>
        /// <param name="id"> Id сущности. </param>
        /// <returns> Cущность. </returns>
        T? GetById(TPrimaryKey id);

        /// <summary>
        /// Получить сущность по Id.
        /// </summary>
        /// <param name="id"> Id сущности. </param>
        /// <param name="cancellationToken"></param>
        /// <returns> Cущность. </returns>
        Task<T?> GetByIdAsync(TPrimaryKey id, CancellationToken cancellationToken);

        /// <summary>
        /// Получить сущность по Id со связанными объектами.
        /// </summary>
        /// <param name="id">Id сущности.</param>
        /// <param name="includeProperties">массив делегатов</param>
        /// <returns></returns>
        Task<T?> GetByIdWithLinkedItemsAsync(TPrimaryKey id, CancellationToken cancellationToken, params Expression<Func<T, object>>[] includeProperties);

        /// <summary>
        /// Добавить в базу одну сущность.
        /// </summary>
        /// <param name="entity"> Сущность для добавления. </param>
        /// <returns> Добавленная сущность. </returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Обновить в базе одну сущность.
        /// </summary>
        /// <param name="entity"> Обновленная сущность. </param>
        /// <returns></returns>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Удалить в базе одну сущность.
        /// </summary>
        /// <param name="id"> Id удаляемой сущности. </param>
        /// <returns></returns>
        Task DeleteAsync(TPrimaryKey id);
    }
}
