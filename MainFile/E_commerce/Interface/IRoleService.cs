using E_commerce.Models;

namespace E_commerce.Interface
{
    public interface IRoleService
    {
        public Task<List<Role>> GetAllRoleAsync();
        public Task CreateRoleAsync(Role role);
        public Task<List<string>> FindRoleNamesByRoleIdsAsync(List<string> ids);
    }
}
