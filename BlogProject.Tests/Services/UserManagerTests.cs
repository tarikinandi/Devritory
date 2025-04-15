using BlogProject.Business.Concrete;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using Moq;
using Xunit;

namespace BlogProject.Tests.Business
{
    public class UserManagerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly UserManager _userManager;

        public UserManagerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _userManager = new UserManager(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, UserName = "tarik" },
                new User { Id = 2, UserName = "ahmet" }
            };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _userManager.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.UserName == "tarik");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser()
        {
            var user = new User { Id = 3, UserName = "demo" };
            _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(user);

            var result = await _userManager.GetByIdAsync(3);

            Assert.NotNull(result);
            Assert.Equal("demo", result.UserName);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser()
        {
            var user = new User { Email = "test@example.com", UserName = "test" };
            _mockRepo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

            var result = await _userManager.GetByEmailAsync("test@example.com");

            Assert.NotNull(result);
            Assert.Equal("test", result.UserName);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldReturnUser()
        {
            var user = new User { UserName = "admin" };
            _mockRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

            var result = await _userManager.GetByUsernameAsync("admin");

            Assert.Equal("admin", result.UserName);
        }

        [Theory]
        [InlineData("a@a.com", "123456", true)]
        [InlineData("a@a.com", "wrong", false)]
        public async Task CheckUserCredentialsAsync_ShouldVerify(string email, string password, bool expected)
        {
            var user = new User { Email = "a@a.com", Password = "123456" };
            _mockRepo.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);

            var result = await _userManager.CheckUserCredentialsAsync(email, password);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallAdd()
        {
            var user = new User { UserName = "newuser" };

            await _userManager.CreateAsync(user);

            _mockRepo.Verify(r => r.AddAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallUpdate_AndSave()
        {
            var user = new User { Id = 5, UserName = "updateuser" };

            await _userManager.UpdateAsync(user);

            _mockRepo.Verify(r => r.Update(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_AndSave()
        {
            var user = new User { Id = 6, UserName = "deleteuser" };

            await _userManager.DeleteAsync(user);

            _mockRepo.Verify(r => r.Delete(user), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
