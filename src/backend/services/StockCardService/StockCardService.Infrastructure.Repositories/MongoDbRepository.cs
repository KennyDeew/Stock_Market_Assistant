using MongoDB.Driver;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Interfaces;
using System.Linq.Expressions;

namespace StockMarketAssistant.StockCardService.Infrastructure.Repositories
{
    public class MongoDbRepository<T, TPrimaryKey> : IMongoRepository<T, TPrimaryKey>
        where T : IEntity<TPrimaryKey>
    {
        private readonly IMongoCollection<T> _collection;

        public MongoDbRepository(IMongoDBContext context)
        {
            _collection = context.GetCollection<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T> GetByIdAsync(TPrimaryKey id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetRangeByIdsAsync(List<TPrimaryKey> ids)
        {
            var filter = Builders<T>.Filter.In(e => e.Id, ids);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T> GetFirstWhere(Expression<Func<T, bool>> predicate)
        {
            var filter = Builders<T>.Filter.Where(predicate);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetWhere(Expression<Func<T, bool>> predicate)
        {
            var filter = Builders<T>.Filter.Where(predicate);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task DeleteAsync(TPrimaryKey id)
        {
            var filter = Builders<T>.Filter.Eq(e => e.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
    }
}
