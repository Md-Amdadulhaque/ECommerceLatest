using E_commerce.Models;

namespace E_commerce.Interface
{
    public interface IProductService
    {

        public Task<List<Product>> GetAsync();

        public Task<List<Product>> GetAsync(int pageNumber,int pageSize);

        public Task<Product?> GetWithIdAsync(string id);

        public Task CreateAsync(Product newProduct);

        public Task<List<Product>> GetTopCheapestAsync(int count = 1);

        public Task UpdateAsync(string id, Product product);
    
        public Task RemoveAsync(string id);
        

    }
}
