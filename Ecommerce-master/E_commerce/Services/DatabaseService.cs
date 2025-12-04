using E_commerce.Interface;
using E_commerce.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_commerce.Services
{
    public class DatabaseService<T> : IDatabaseService<T> where T : class
    {

        private IMongoDbContext<T> _database;

        private IMongoCollection<T> _collection;


        public DatabaseService(IMongoDbContext<T> context)
        {
            _database = context;
        }

        public async Task SetCollection(string collectionName)
        {
            _collection = _database.GetCollection<T>(collectionName);
            return;
        }


        public async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<T>> GetByFilterAsync(FilterDefinition<T> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T> FindAsync(string id)
        {

            return await _collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync();

        }

        public async Task<T> FindAsync(string userName, string password)
        {
            var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Eq("UserName", userName),
            Builders<T>.Filter.Eq("Password", password));
            return await _collection.Find(filter).FirstOrDefaultAsync();

        }

        public async Task AddAsync(T newData)
        {
            await _collection.InsertOneAsync(newData);
        }

        public async Task UpdateAsync(string id, T newData)
        {
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("Id", id), newData);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id));
        }

        public async Task UpdateAsyncWithFilter(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            await _collection.UpdateOneAsync(filter, update);
        }
        public async Task<List<T>> GetBySortThenFilterAsync(SortDefinition<T> sort, FilterDefinition<T> filter, int limit)
        {
            return await _collection.Aggregate()
                .Sort(sort)
                .Match(filter)
                .Limit(limit)
                .ToListAsync();
        }



    }
}