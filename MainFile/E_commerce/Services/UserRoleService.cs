using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;


namespace E_commerce.Services

{
    public class UserRoleService:IUserRoleService
    {
        private IDatabaseService<UserRoles> _databaseService;
        private IRoleService _roleService;

       public UserRoleService(IDatabaseService<UserRoles> databaseService,IRoleService roleService)
        {
            _databaseService = databaseService;
            _databaseService.SetCollection(nameof(UserRoles));
            _roleService = roleService; 

            
        }

        public async Task CreateUserRoleAsync(UserRoles userRole)
        {
            await _databaseService.AddAsync(userRole);
        } 
        public async Task<List<UserRoles>> GetAllUserRoleAsync()
        {
            return await _databaseService.GetAllAsync();

        }


        public async Task<List<string>> GetRoleNamesByUserIdAsync(string userId)
        {
            if (userId == null) {
                throw new ArgumentNullException(nameof(userId));
            }

            //var salesView = _databaseService.GetCollection<BsonDocument>("sales");
            //var filter = Builders<BsonDocument>.Filter.Empty; // No filter to get all documents
            //var salesDocuments = salesView.Find(filter).ToList();


            var userRoles = await _databaseService.GetAllAsync();
            
            List<string> roleIds = new List<string>();
            for (int i = 0; i < userRoles.Count; i++) {
                if (userRoles[i].UserId == userId) { 
                   roleIds.Add(userRoles[i].RoleId.ToString());
                }
            }
            List<string> roles = await _roleService.FindRoleNamesByRoleIdsAsync(roleIds);

            return roles;

             
        }

    }
}
