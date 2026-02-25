using E_commerce.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZstdSharp.Unsafe;
using E_commerce.Models;
using E_commerce.Services;
namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        public UserRoleController(IUserRoleService userRoleService) { 
              _userRoleService = userRoleService;
        }

        [HttpPost("/CreateUserRole")]
        public async Task CreateRole([FromBody] UserRoles userRole)
        {
            await _userRoleService.CreateUserRoleAsync(userRole);
        }
        [HttpGet("/GetUserRole")]

        public async Task<List<UserRoles>> GetUserRole()
        {
            return await _userRoleService.GetAllUserRoleAsync();
        }

    }
}
