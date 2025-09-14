using E_commerce.Models;

namespace E_commerce.Interface
{
    public interface IUserRoleService
    {
      public Task<List<string>> GetRoleNamesByUserIdAsync(string userId);
        public Task<List<UserRoles>> GetAllUserRoleAsync();
        public Task CreateUserRoleAsync(UserRoles userRole);
    }
}
