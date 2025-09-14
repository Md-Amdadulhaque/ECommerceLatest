using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_commerce.Services
{
    public class RoleService:IRoleService
    {
        private IDatabaseService<Role> _databaseService;


        public RoleService(IDatabaseService<Role> databaseService)
        {
            _databaseService = databaseService;
            _databaseService.SetCollection(nameof(Role));
        }

        public async Task CreateRoleAsync(Role role)
        {
            await _databaseService.AddAsync(role);
        }

        public async Task<List<Role>> GetAllRoleAsync()
        {
            return await _databaseService.GetAllAsync();
        }

        public async Task<List<string>> FindRoleNamesByRoleIdsAsync(List<string> ids)
        {
            Dictionary<string, bool> map = new Dictionary<string, bool>();

            var filter = Builders<Role>.Filter.In(doc => doc.Id, ids);
            var results = await _databaseService.GetByFilterAsync(filter);


            List<string> roleNames = new List<string>();
            for (int i = 0; i < results.Count; i++)
            { roleNames.Add(results[i].Name); }
            
            
            return roleNames;

        }
    }
}
