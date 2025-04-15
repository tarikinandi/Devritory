using BlogProject.Business.Concrete;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogProject.Tests.Business
{
    public class CategoryManagerTests
    {
        private readonly Mock<ICategoryRepository> _mockRepo;
        private readonly CategoryManager _categoryManager;

        public CategoryManagerTests()
        {
            _mockRepo = new Mock<ICategoryRepository>();
            _categoryManager = new CategoryManager(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Yazılım" },
                new Category { Id = 2, Name = "Teknoloji" }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _categoryManager.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Teknoloji");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCategory()
        {
            var category = new Category { Id = 1, Name = "Yazılım" };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

            var result = await _categoryManager.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Yazılım", result.Name);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallAddAndSave()
        {
            var category = new Category { Name = "YeniKategori" };

            await _categoryManager.CreateAsync(category);

            _mockRepo.Verify(r => r.AddAsync(category), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallUpdateAndSave()
        {
            var category = new Category { Id = 2, Name = "Güncelleme" };

            await _categoryManager.UpdateAsync(category);

            _mockRepo.Verify(r => r.Update(category), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAndSave()
        {
            var category = new Category { Id = 3, Name = "Silinecek" };

            await _categoryManager.DeleteAsync(category);

            _mockRepo.Verify(r => r.Delete(category), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }
    }
}
