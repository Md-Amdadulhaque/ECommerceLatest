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
            var allCart = await _databaseService.GetAllAsync();
            return allCart.FirstOrDefault(c => c.UserId == userId);
        }

        public async Task CreateCartAsync(string userId)
        {   var model = new Cart();
            model.UserId = userId;
            await _databaseService.AddAsync(model);
            
        }

        public async Task AddItemToCartAsync(string userId, CartItem cartItem)
        {
            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
            var update = Builders<Cart>.Update.Push(c => c.Items, cartItem);
            await _databaseService.UpdateAsyncWithFilter(filter, update);
        }

        //public async Task<decimal> GetTotalAmount()
        //{
        //    decimal total = 0;
        //    var allCart = await GetItems();
        //    for (int i = 0; i < allCart.Count; i++)
        //    {
        //        total += allCart[i].TotalPrice;
        //    }
        //    return total;
        //}

        //public async Task AddToCart(string productId, string productName, int quantity, decimal unitPrice)
        //{
        //    var allValue = await _databaseService.GetAllAsync();

        //    //Needed to change here,, i have get all the list and check but not sure is it updating or not?

        //    var existingItem = allValue.FirstOrDefault(x => x.ProductId == productId);
        //    for (int i = 0; i < allValue.Count; i++)
        //    {
        //        if (allValue[i].ProductId == productId)
        //        {
        //            allValue[i].Quantity += quantity;
        //            return;
        //        }
        //    }

        //    if (existingItem != null)
        //    {
        //        existingItem.Quantity += quantity;
        //    }
        //    else
        //    {
        //        var newItem = new CartItem
        //        {
        //            ProductId = productId,
        //            ProductName = productName,
        //            Quantity = quantity,
        //            UnitPrice = unitPrice
        //        };
        //        _databaseService.AddAsync(newItem);
        //    }
        //}

        public void UpdateQuantity(string productId, int quantity)
        {
            //var filter = Builders<CartItem>.Filter.Eq(x => x.ProductId, productId);
            //if (quantity <= 0)
            //{
            //    _databaseService.(filter);
            //}
            //else
            //{
            //    var update = Builders<CartItem>.Update.Set(i => i.Quantity, quantity);
            //    _databaseService.UpdateOne(filter, update);
            //}
        }

        public void RemoveFromCart(int productId)
        {
            //var filter = Builders<CartItem>.Filter.Eq(i => i.ProductId, productId);
            //_cartCollection.DeleteOne(filter);
        }

        public async void ClearCart()
        {
            //await _databaseService.DeleteAsync();
        }
    }
    
}

