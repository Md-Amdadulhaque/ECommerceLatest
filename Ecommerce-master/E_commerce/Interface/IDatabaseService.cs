using MongoDB.Driver;

namespace E_commerce.Interface
{
    public interface IDatabaseService<T> where T : class
    {

        public Task SetCollection(string collectionName);
        public Task<List<T>> GetAllAsync();
        public Task<List<T>> GetByFilterAsync(FilterDefinition<T> filter);
        public Task<List<T>> GetBySortThenFilterAsync(SortDefinition<T> sort, FilterDefinition<T> filter, int limit);
        public Task<T> FindAsync(string id);
        public Task<T> FindAsync(string userName, string password);
        public Task AddAsync(T newData);
        public Task UpdateAsync(string id, T newData);
        public Task DeleteAsync(string id);
        public Task DeleteAsync(FilterDefinition<T> filter);
        public Task UpdateAsyncWithFilter(FilterDefinition<T> filter, UpdateDefinition<T> update);
        public Task<T> GetItemByFilterAsync(FilterDefinition<T> filter);
        


        }
}