using BlogProject.Entity;
using System.Threading.Tasks;

namespace BlogProject.Business.Abstract
{
    public interface IBlogService
    {
        Task<List<Blog>> GetAllAsync();
        Task<List<Blog>> GetAllActiveAsync();
        Task<List<Blog>> GetAllByUserIdAsync(int userId); 
        Task<Blog?> GetByIdAsync(int id);
        Task AddAsync(Blog blog);
        Task UpdateAsync(Blog blog);
        void Delete(Blog blog);

        Task<List<Blog>> GetAllWithCategoriesAsync();
        Task<Blog?> GetBlogWithUserAndCategoriesAsync(int id);
        Task<List<Blog>> GetByCategoryIdAsync(int categoryId);
        Task<List<Blog>> GetActiveByCategoryIdAsync(int categoryId);
        Task<Blog?> GetBlogBySlugAsync(string slug);
    }
}
