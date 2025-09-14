using E_commerce.Models;
using E_commerce.Services;
using MongoDB.Driver;

namespace E_commerce.Interface
{
    public interface ICartService
    {
        public Task<Cart> GetCartAsync(string userId);
        public Task CreateCartAsync(string userId);

        public Task AddItemToCartAsync(string userId, CartItem cartItem);
        
    }
}
