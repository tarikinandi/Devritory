using BlogProject.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlogProject.DataAccess.Abstract
{
    public interface IBlogRepository : IGenericRepository<Blog>
    {
        Task<Blog?> GetBlogWithUserAndCategoriesAsync(int id);
        Task<List<Blog>> GetAllWithCategoriesAsync();
        Task<List<Blog>> GetByCategoryIdAsync(int categoryId);
        Task<List<Blog>> GetActiveBlogsAsync();
        Task<List<Blog>> GetBlogsByUserIdAsync(int userId);
        Task<Blog?> GetBlogBySlugAsync(string slug);

}
}
