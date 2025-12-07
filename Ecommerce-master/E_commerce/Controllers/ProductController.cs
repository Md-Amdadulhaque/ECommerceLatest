using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_commerce.Interface;
using MongoDB.Bson;

namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<List<Product>> Get() =>
            await _productService.GetAsync();

        [HttpPost("FilterBy")]
        public async Task<List<Product>> Get([FromBody] RequestItem Req) =>
            await _productService.GetAsync(Req.PageIndex, Req.PageSize);

        [HttpPost("GetById")]
        public async Task<ActionResult<Product>> Get([FromBody] ID id)
        {
            var product = await _productService.GetWithIdAsync(id.Id);
            return product;
        }

        [HttpPost("GetByFilterRequestModel")]
        public async Task<ActionResult<List<Product>>> Get([FromBody] ProductFilterRequest productFilterRequest)
        {
            var product = await _productService.GetWithFilter(productFilterRequest);
            return product;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product newProduct)
        {
            await _productService.CreateAsync(newProduct);
            return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Product updatedProduct)
        {
            var product = await _productService.GetWithIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            updatedProduct.Id = product.Id;

            await _productService.UpdateAsync(id, updatedProduct);
            return NoContent();
        }
        [HttpGet("category/{category}")]
        public async Task<List<Product>> GetAllProductByCategory(string category)
        {
            var products = await _productService.GetByCategoryAsync(category);
            return products??new List<Product>();
        }

        [HttpGet("cheapest")]
        public async Task<IActionResult> GetCheapest([FromQuery] int count = 1)
        {
            if (count <= 0) return BadRequest("count must be greater than 0");

            var products = await _productService.GetTopCheapestAsync(count);
            return Ok(products);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetWithIdAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            await _productService.RemoveAsync(id);

            return Ok();
        }
    }
}