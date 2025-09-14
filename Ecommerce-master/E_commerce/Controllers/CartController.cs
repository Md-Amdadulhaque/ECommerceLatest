using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private ICartService _cartService;
        private IUserService _userService;
        private IProductService _productService;
        public CartController(ICartService cartService,IUserService userService,IProductService productService)
        {
            _cartService = cartService;
            _userService = userService;
            _productService = productService;
        }
        [HttpPost("GetCart")]
        public async Task<List<CartItem>> GetCartAsync([FromBody] UserID userId)
        {
            var cart = await _cartService.GetCartAsync(userId.Id);
            var item = cart.Items;
            return item;

        }

        [HttpPost("CreateCart")]
        public async Task CreateCartAsync([FromBody] UserID userId)
        {
             await _cartService.CreateCartAsync(userId.Id);
            

        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddtoCartAsync([FromBody] AddToCart addToCart)
        {
            string userId = addToCart.UserId;
            string productId = addToCart.ProductId;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
                return BadRequest("Invalid parameters.");

            var product = await _productService.GetWithIdAsync(productId);

            var cartitem = new CartItem();
            cartitem.ProductId = product.Id;
            cartitem.ProductName = product.Name;
            //  cartitem.UnitPrice = product.Price;

            await _cartService.AddItemToCartAsync(userId, cartitem);
            return Ok();
        }

    }
}
