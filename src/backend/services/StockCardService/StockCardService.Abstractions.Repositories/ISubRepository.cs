using StockMarketAssistant.StockCardService.Domain.Interfaces;

namespace StockCardService.Abstractions.Repositories
{
    /// <summary>
    /// Описания общих методов для всех репозиториев.
    /// </summary>
    /// <typeparam name="T"> Тип сущности для репозитория </typeparam>
    /// <typeparam name="TPrimaryKey"> Тип идентификатора сущности </typeparam>
    public interface ISubRepository<T, TPrimaryKey>
        : IRepository<T, TPrimaryKey>
        where T : IEntityWithParent<TPrimaryKey>, IEntity<TPrimaryKey>
    {
        /// <summary>
        /// Запросить все сущности в базе по Id родителя.
        /// </summary>
        Task<List<T>> GetAllByParentIdAsync(
            TPrimaryKey id,
            CancellationToken cancellationToken,
            bool asNoTracking = false
        );
    }
}
