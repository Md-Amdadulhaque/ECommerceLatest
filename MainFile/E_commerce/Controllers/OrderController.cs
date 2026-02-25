using E_commerce.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        [HttpPost("ConfirmOder")]
        public async Task<IActionResult> ConfirmOrder(Order order)
        {
            return Ok();

        }
        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(Order order)
        {
            return Ok();
        }
    }
}
