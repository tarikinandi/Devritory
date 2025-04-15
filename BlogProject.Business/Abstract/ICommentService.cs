using BlogProject.Entity;

namespace BlogProject.Business.Abstract
{
    public interface ICommentService
    {
        Task<List<Comment>> GetAllAsync();
        Task<Comment> GetByIdAsync(int id);
        Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId);
        Task CreateAsync(Comment comment);
        Task DeleteAsync(Comment comment);
        Task<List<Comment>> GetAllWithBlogAndUserAsync();
        Task UpdateAsync(Comment comment);
        Task AddCommentAsync(Comment comment);
        Task<List<Comment>> FetchCommentsForBlogAsync(int blogId);
    }
}
