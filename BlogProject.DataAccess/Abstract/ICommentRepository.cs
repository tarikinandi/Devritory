using BlogProject.Entity;

namespace BlogProject.DataAccess.Abstract
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId);
        Task<List<Comment>> GetAllWithBlogAndUserAsync();
        Task DeleteAsync(Comment comment);
    }
}
