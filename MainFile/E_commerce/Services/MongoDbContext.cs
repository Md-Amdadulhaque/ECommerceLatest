using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace E_commerce.Services
{
    public class MongoDbContext<T>:IMongoDbContext<T> where T : class
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IOptions<MongoDbContextSetting> MongoDbContextSettings)
        {
        var mongoClient = new MongoClient(
            MongoDbContextSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            MongoDbContextSettings.Value.DatabaseName);

            _database = mongoDatabase;
        }


        public IMongoCollection<T> GetCollection<T>(string collectionName) { 
            return _database.GetCollection<T>(collectionName);
        }

    }
}
