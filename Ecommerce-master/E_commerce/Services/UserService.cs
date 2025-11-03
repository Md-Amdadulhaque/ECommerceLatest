using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data.SqlTypes;


namespace E_commerce.Services
{
    public class UserService : IUserService
    {
        private IDatabaseService<User> _databaseService;
        private IUserRoleService _userRoleService;
        private IRoleService _roleService;

        private IConfiguration _configuration;
       public UserService(IDatabaseService<User> databaseService, IConfiguration configuration,
           IUserRoleService userRoleService,IRoleService roleService)
        {
            _databaseService = databaseService;
            _databaseService.SetCollection(nameof(User));
            _configuration = configuration;
            _roleService = roleService;
            _userRoleService = userRoleService;
           
        }

        public async Task<List<User>> GetAsync()
        {
            return await _databaseService.GetAllAsync();
        }

        public async Task<string> GetIdByUserAsync(string name)
        {
            var all = await _databaseService.GetAllAsync();
            var id = all.FirstOrDefault(x=>x.UserName == name);
            return id == null ? null : id.Id;
        }

        public async Task RegisterUserAsync(User user)
        {   if(user.Id!=null)
            { await _databaseService.AddAsync(user); }
        }

        public async Task UnRegisterUserAsync(User user)
        {
            await _databaseService.DeleteAsync(user?.Id);
        }

        public async Task<string> GetTokenAsync(string userName, string password)
        {
            var user = await _databaseService.FindAsync(userName, password);

            if (user == null)
            {
                return null;
            }
            //return user.Id+user.UserName;
            var roles = await _userRoleService.GetRoleNamesByUserIdAsync(user.Id);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

       

     
    }
}