using E_commerce.Interface;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<List<User>> Get() =>
        await _userService.GetAsync();

        [HttpGet("GetUserByUser")]
        public async Task<ActionResult<User?>> GetUserAsync([FromQuery] string name, [FromQuery] string email)
        {
            var user = new User { UserName = name, Email = email };
            return Ok(await _userService.GetUserByUserAsync(user));
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {   
          
            var alluser = await _userService.GetAsync();
            var user1 = alluser.FirstOrDefault(x=>x.Email==user.Email||x.UserName==user.UserName);

            if (user1 != null) {
                return BadRequest(new { message = "Email or UserName already in use" });
            }

            await _userService.RegisterUserAsync(user);

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpPost("DeleteUser")]
        public async Task DeleteUser([FromBody] User user)
        {
            await _userService.UnRegisterUserAsync(user);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var token = await _userService.GetTokenAsync(login.UserName, login.Password);
            var userId = await _userService.GetIdByUserAsync(login.UserName);
            if (token == null) return Unauthorized(new { message = "Invalid credentials" });
            return Ok( new { token = token, userId = userId });
        }


    }
}