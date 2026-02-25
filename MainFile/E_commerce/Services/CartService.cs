using E_commerce.Interface;
using E_commerce.Models;
using MongoDB.Driver;

namespace E_commerce.Services
{
    public class CartService:ICartService
    {
        private IDatabaseService<Cart> _databaseService;
        public CartService(IDatabaseService<Cart> databaseService)
        {
            _databaseService = databaseService;
            _databaseService.SetCollection(nameof(Cart));
        }
        public async Task<Cart> GetCartAsync(string userId)
        {
            var filter = Builders<Cart>.Filter.Eq("UserId", userId);
            var cart = await _databaseService.GetItemByFilterAsync(filter);
            return cart;
        }
        public async Task CreateCartAsync(string userId)
        {   var model = new Cart();
            model.UserId = userId;
            await _databaseService.AddAsync(model);
            
        }
        public async Task DeleteCartAsync(string userId)
        {
            var filter = Builders<Cart>.Filter.Eq("UserId", userId);
            await _databaseService.DeleteAsync(filter);

        }

        public async Task AddItemToCartAsync(string userId, CartItem cartItem)
        {
            cartItem.Quantity++;
            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);

            var cart = await GetCartAsync(userId);
            var item = cart?.Items?.FirstOrDefault(e => e.ProductId == cartItem.ProductId);
            if (item != null)
            {
                cartItem.Quantity = item.Quantity + 1;
            }
            var update = Builders<Cart>.Update.Push(c => c.Items, cartItem);
            await _databaseService.UpdateAsyncWithFilter(filter, update);
        }

        public async Task InitiatePayment(string userId)
        {
            var filter = Builders<Cart>.Filter.Eq("UserId", userId);
            var cart = await _databaseService.GetItemByFilterAsync(filter);
            var totalPrice = GetTotalAmount(cart);
            /// PaymentDone
            /// 
            await DeleteCartAsync(userId);
        }
        private Decimal GetTotalAmount(Cart cart)
        {
            decimal total = 0;
            foreach (var item in cart.Items)
            {
                total += item.TotalPrice;
            }
            return total;
        }
        public void RemoveFromCart(int productId)
        {
            //var filter = Builders<CartItem>.Filter.Eq(i => i.ProductId, productId);
            //_cartCollection.DeleteOne(filter);
        }
    }
    
}

