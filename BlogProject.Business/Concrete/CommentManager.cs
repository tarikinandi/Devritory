using BlogProject.Business.Abstract;
using BlogProject.DataAccess.Abstract;
using BlogProject.DataAccess.Concrete.Repositories;
using BlogProject.Entity;

namespace BlogProject.Business.Concrete
{
    public class CommentManager : ICommentService
    {
        private readonly ICommentRepository _repository;

        public CommentManager(ICommentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Comment>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Comment> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId) => await _repository.GetCommentsByBlogIdAsync(blogId);

        public async Task CreateAsync(Comment comment) => await _repository.AddAsync(comment); 

        public async Task DeleteAsync(Comment comment){
            await _repository.DeleteAsync(comment);

        }

        public async Task<List<Comment>> GetAllWithBlogAndUserAsync() => await _repository.GetAllWithBlogAndUserAsync();

        public async Task UpdateAsync(Comment comment)
        {
            _repository.Update(comment);
            await _repository.SaveAsync();
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _repository.AddAsync(comment);
        }

        public async Task<List<Comment>> FetchCommentsForBlogAsync(int blogId) => await _repository.GetCommentsByBlogIdAsync(blogId);

    }
}
