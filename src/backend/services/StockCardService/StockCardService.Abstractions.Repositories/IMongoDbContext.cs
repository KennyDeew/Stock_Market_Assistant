using MongoDB.Driver;

namespace StockCardService.Abstractions.Repositories
{
    public interface IMongoDBContext
    {
        IMongoDatabase Database { get; }
        IMongoCollection<T> GetCollection<T>(string? name = null);
    }
}
