using BlogProject.Business.Concrete;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BlogProject.Tests.Business
{
    public class BlogManagerTests
    {
        private readonly BlogManager _blogManager;
        private readonly Mock<IBlogRepository> _mockRepo;

        public BlogManagerTests()
        {
            _mockRepo = new Mock<IBlogRepository>();
            _blogManager = new BlogManager(_mockRepo.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Call_Repo_Add_And_Save()
        {
            var blog = new Blog { Title = "Test", Content = "Content" };

            await _blogManager.AddAsync(blog);

            _mockRepo.Verify(r => r.AddAsync(blog), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public void Delete_Should_Call_Repo_Delete_And_Save()
        {
            var blog = new Blog { Id = 1 };

            _blogManager.Delete(blog);

            _mockRepo.Verify(r => r.Delete(blog), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Blogs()
        {
            var expected = new List<Blog> { new Blog { Title = "T1" } };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

            var result = await _blogManager.GetAllAsync();

            Assert.Single(result);
            Assert.Equal("T1", result[0].Title);
        }

        [Fact]
        public async Task GetByCategoryIdAsync_Should_Filter_By_Category()
        {
            var category = new Category { Id = 1, Name = "Tech" };
            var blogWithCat = new Blog { Id = 1, Categories = new List<Category> { category } };
            var blogNoCat = new Blog { Id = 2, Categories = new List<Category>() };

            _mockRepo.Setup(r => r.GetAllWithCategoriesAsync())
                     .ReturnsAsync(new List<Blog> { blogWithCat, blogNoCat });

            var result = await _blogManager.GetByCategoryIdAsync(1);

            Assert.Single(result);
            Assert.Contains(result, b => b.Id == 1);
        }

        [Fact]
        public async Task GetActiveByCategoryIdAsync_Should_Filter_By_Category_And_IsActive()
        {
            var category = new Category { Id = 1 };
            var activeBlog = new Blog { Id = 1, IsActive = true, Categories = new List<Category> { category } };
            var inactiveBlog = new Blog { Id = 2, IsActive = false, Categories = new List<Category> { category } };

            _mockRepo.Setup(r => r.GetAllWithCategoriesAsync())
                     .ReturnsAsync(new List<Blog> { activeBlog, inactiveBlog });

            var result = await _blogManager.GetActiveByCategoryIdAsync(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task GetBlogBySlugAsync_Should_Return_Correct_Blog()
        {
            var blog = new Blog { Slug = "example-blog" };
            _mockRepo.Setup(r => r.GetBlogBySlugAsync("example-blog"))
                     .ReturnsAsync(blog);

            var result = await _blogManager.GetBlogBySlugAsync("example-blog");

            Assert.Equal("example-blog", result?.Slug);
        }
    }
}
