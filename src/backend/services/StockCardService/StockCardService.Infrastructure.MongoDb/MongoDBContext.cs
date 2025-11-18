using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Infrastructure.MongoDb.Settings;

namespace StockMarketAssistant.StockCardService.Infrastructure.MongoDb
{
    public class MongoDBContext : IMongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(IOptions<MongoSettings> settings)
        {
            //Настройка сериализации. Нужна для корректной работы фильтров (MongoDbRepository - GetFirstWhere - Filter). Mongo хранит ParentId в одном формате, а запрос (x => x.ParentId == id) конвертируется в другой → и фильтр ничего не находит.
            // Глобальная настройка сериализации Guid в формате Standard (RFC 4122)
            BsonSerializer.TryRegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));
            // Создаём настройки клиента на основе connection string
            var mongoSettings = MongoClientSettings.FromConnectionString(settings.Value.ConnectionString);

            //клиент (один на приложение: singleton). Нужен для связи с сервером и работы с БД.
            var client = new MongoClient(settings.Value.ConnectionString);
            //по имени получаем БД
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        //короткая запись для свойства с геттером
        public IMongoDatabase Database => _database;

        public IMongoCollection<T> GetCollection<T>(string? name = null)
        {
            // Если name null или пустая строка, используем имя типа + "s"
            var collectionName = !string.IsNullOrWhiteSpace(name)
                ? name
                : typeof(T).Name + "s";

            // Возвращаем коллекцию
            return _database.GetCollection<T>(collectionName);
        }
    }
}
