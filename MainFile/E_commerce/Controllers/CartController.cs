using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IUserService userService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        /// <summary>
        /// Retrieves all items in the user's cart.
        /// </summary>
        [HttpPost("GetCart")]
        public async Task<ActionResult<List<CartItem>>> GetCartAsync([FromBody] UserID userId)
        {
            var cart = await _cartService.GetCartAsync(userId.Id);
            var items = cart?.Items;

            if (items == null)
                return NotFound("Cart not found.");

            return Ok(items);
        }

        /// <summary>
        /// Creates an empty cart for the user.
        /// </summary>
        [HttpPost("CreateCart")]
        public async Task<IActionResult> CreateCartAsync([FromBody] UserID userId)
        {
            await _cartService.CreateCartAsync(userId.Id);
            return Ok("Cart created successfully.");
        }

        /// <summary>
        /// Initiates payment for the user's cart.
        /// </summary>
        [HttpPost("InitiatePayment")]
        public async Task<IActionResult> InitiatePayment([FromBody] UserID userId)
        {
            await _cartService.InitiatePayment(userId.Id);
            return Ok("Payment initiated successfully.");
        }

        /// <summary>
        /// Adds a product to the user's cart.
        /// </summary>
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCartAsync([FromBody] AddToCart addToCart)
        {
            if (string.IsNullOrEmpty(addToCart.UserId) || string.IsNullOrEmpty(addToCart.ProductId))
                return BadRequest("Invalid parameters.");

            var product = await _productService.GetWithIdAsync(addToCart.ProductId);
            if (product == null)
                return NotFound("Product not found.");

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                ImageUrl = product.ImageData
            };

            await _cartService.AddItemToCartAsync(addToCart.UserId, cartItem);
            return Ok("Item added to cart.");
        }

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        [HttpGet("CartClear/{userId}")]
        public async Task<IActionResult> DeleteCartItem(string userId)
        {
            await _cartService.DeleteCartAsync(userId);
            return Ok("Cart cleared successfully.");
        }
    }
}
