using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using E_commerce.Models;
using E_commerce.Interface;
using Microsoft.AspNetCore.Authorization;
namespace E_commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {

        private IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService; 
        }
        
        [HttpGet("GetRole")]
        public async Task<List<Role>> Get() {
            return await _roleService.GetAllRoleAsync();
                
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddRole")]
        public async Task AddRole([FromBody] Role role)
        {
            await _roleService.CreateRoleAsync(role);
        }
    }
}
