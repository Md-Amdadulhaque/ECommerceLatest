using E_commerce.Models;

namespace E_commerce.Interface
{
    public interface ICategoryService
    {
        public Task<List<Category>> GetAsync();

        public Task<List<Category>> GetAsync(int pageNumber,int pageSize);

        public Task<Category?> GetAsync(string id);

        public Task CreateAsync(Category newCategory);

        public Task UpdateAsync(string id, Category updatedCategory);

        public Task RemoveAsync(string id);
    }
}
