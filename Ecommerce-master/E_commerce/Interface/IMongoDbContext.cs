using MongoDB.Driver;

namespace E_commerce.Interface
{
    public interface IMongoDbContext<T> where T : class
    {
        public IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
