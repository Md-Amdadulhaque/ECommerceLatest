using E_commerce;
using E_commerce.Interface;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace E_Commerce_Test
{
    public class UnitTest1
    {
        private readonly Mock<IDatabaseService<User>> _mockDatabaseService;
        private readonly Mock<IUserRoleService> _mockUserRoleService;
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _userService;

        public UnitTest1()
        {
            _mockDatabaseService = new Mock<IDatabaseService<User>>();
            _mockUserRoleService = new Mock<IUserRoleService>();
            _mockRoleService = new Mock<IRoleService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _userService = new UserService(_mockDatabaseService.Object,_mockConfiguration.Object,_mockUserRoleService.Object,_mockRoleService.Object);
        
        }
        
        [Fact]


        public async Task Test1()
        {
            var user = new User();
            await _userService.RegisterUserAsync(user);
            Assert.Equal(1, 0);
        }
    }
}