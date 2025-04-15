using BlogProject.Business.Concrete;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using Moq;
using Xunit;

namespace BlogProject.Tests.Business
{
    public class CommentManagerTests
    {
        private readonly Mock<ICommentRepository> _mockRepo;
        private readonly CommentManager _commentManager;

        public CommentManagerTests()
        {
            _mockRepo = new Mock<ICommentRepository>();
            _commentManager = new CommentManager(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCommentList()
        {
            var comments = new List<Comment>
            {
                new Comment { Id = 1, Content = "Yorum 1" },
                new Comment { Id = 2, Content = "Yorum 2" }
            };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(comments);

            var result = await _commentManager.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Content.Contains("Yorum"));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnComment()
        {
            var comment = new Comment { Id = 5, Content = "Test Yorum" };
            _mockRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(comment);

            var result = await _commentManager.GetByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal("Test Yorum", result.Content);
        }

        [Fact]
        public async Task GetCommentsByBlogIdAsync_ShouldReturnBlogComments()
        {
            var comments = new List<Comment>
            {
                new Comment { Id = 1, BlogId = 3 },
                new Comment { Id = 2, BlogId = 3 }
            };

            _mockRepo.Setup(r => r.GetCommentsByBlogIdAsync(3)).ReturnsAsync(comments);

            var result = await _commentManager.GetCommentsByBlogIdAsync(3);

            Assert.All(result, c => Assert.Equal(3, c.BlogId));
        }

        [Fact]
        public async Task CreateAsync_Should_Call_AddAsync()
        {
            var comment = new Comment { Id = 7, Content = "Yeni yorum" };

            await _commentManager.CreateAsync(comment);

            _mockRepo.Verify(r => r.AddAsync(comment), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Call_Delete_And_Save()
        {
            var comment = new Comment { Id = 9 };

            await _commentManager.DeleteAsync(comment);

            _mockRepo.Verify(r => r.Delete(comment), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Call_Update_And_Save()
        {
            var comment = new Comment { Id = 10, Content = "Güncellenecek yorum" };

            await _commentManager.UpdateAsync(comment);

            _mockRepo.Verify(r => r.Update(comment), Times.Once);
            _mockRepo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllWithBlogAndUserAsync_ShouldReturnWithRelations()
        {
            var comments = new List<Comment>
            {
                new Comment { Id = 1, Content = "Yorum", Blog = new Blog(), User = new User() }
            };

            _mockRepo.Setup(r => r.GetAllWithBlogAndUserAsync()).ReturnsAsync(comments);

            var result = await _commentManager.GetAllWithBlogAndUserAsync();

            Assert.Single(result);
            Assert.NotNull(result[0].Blog);
            Assert.NotNull(result[0].User);
        }
    }
}
