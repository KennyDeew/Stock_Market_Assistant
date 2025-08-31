using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework.Settings;

namespace StockMarketAssistant.StockCardService.Infrastructure.EntityFramework
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(IOptions<MongoSettings> settings)
        {
            //клиент (один на приложение: singleton). Нужен для связи с сервером и работы с БД.
            var client = new MongoClient(settings.Value.ConnectionString);
            //по имени получаем БД
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        //короткая запись для свойства с геттером
        public IMongoDatabase Database => _database;

        public IMongoCollection<T> GetCollection<T>(string name = null)
        {
            var collectionName = name ?? typeof(T).Name + "s";
            return _database.GetCollection<T>(collectionName);
        }
    }
}
