using E_commerce.Models;
using MongoDB.Driver;

namespace E_commerce.Interface
{
    public interface IUserService
    {



        public Task<List<User>> GetAsync();

        public Task RegisterUserAsync(User user);
        public Task UnRegisterUserAsync(User user);

        public Task<string> GetIdByUserAsync(string userName);
        public Task<string> GetTokenAsync(string userName,string password);

        public Task<User?> GetUserByUserAsync(User user);



    }
}