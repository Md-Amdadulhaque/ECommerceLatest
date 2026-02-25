using E_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using E_commerce.Interface;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;


namespace E_commerce.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private ICategoryService _categoryServices;
        public CategoryController(ICategoryService categoryServices)
        {_categoryServices = categoryServices;}

        [HttpGet]
        public async Task<List<Category>> Get() =>
        await _categoryServices.GetAsync();

        [HttpPost("FilterBy")]
        public async Task<List<Category>> Get([FromBody] RequestItem Req) =>
        await _categoryServices.GetAsync(Req.PageIndex, Req.PageSize);

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> Get(string id)
        {
            var category = await _categoryServices.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            return Ok(category);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post(Category newCategory)
        {
            await _categoryServices.CreateAsync(newCategory);

            return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, newCategory);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Category updatedCategory)
        {
            var category = await _categoryServices.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            updatedCategory.Id = category.Id;

            await _categoryServices.UpdateAsync(id, updatedCategory);

            return NoContent();
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _categoryServices.GetAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            await _categoryServices.RemoveAsync(id);

            return Ok();
        }
    }
}