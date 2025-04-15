using BlogProject.Business.Abstract;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using System.Threading.Tasks;

namespace BlogProject.Business.Concrete
{
    public class BlogManager : IBlogService
    {
        private readonly IBlogRepository _blogRepository;

        public BlogManager(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        public async Task AddAsync(Blog blog)
        {
            await _blogRepository.AddAsync(blog);
            await _blogRepository.SaveAsync();
        }
        public void Delete(Blog blog) {
            _blogRepository.Delete(blog); 
            _blogRepository.SaveAsync(); 
        }
        public async Task UpdateAsync(Blog blog) {
            _blogRepository.Update(blog);
            await _blogRepository.SaveAsync();
        }

        public async Task<List<Blog>> GetAllAsync() => await _blogRepository.GetAllAsync();
        public async Task<List<Blog>> GetAllActiveAsync()
        {
            var all = await _blogRepository.GetAllWithCategoriesAsync();
            return all.Where(b => b.IsActive).ToList();
        }

        public async Task<List<Blog>> GetAllByUserIdAsync(int userId)
        {
            var all = await _blogRepository.GetAllWithCategoriesAsync();
            return all.Where(b => b.UserId == userId).ToList();
        }

        public async Task<Blog?> GetByIdAsync(int id) => await _blogRepository.GetByIdAsync(id);

        public async Task<List<Blog>> GetAllWithCategoriesAsync() => await _blogRepository.GetAllWithCategoriesAsync();

        public async Task<Blog?> GetBlogWithUserAndCategoriesAsync(int id) => await _blogRepository.GetBlogWithUserAndCategoriesAsync(id);

        public async Task<List<Blog>> GetByCategoryIdAsync(int categoryId)
        {
            var all = await _blogRepository.GetAllWithCategoriesAsync();
            return all.Where(b => b.Categories.Any(c => c.Id == categoryId)).ToList();
        }

        public async Task<List<Blog>> GetActiveByCategoryIdAsync(int categoryId)
        {
            var all = await GetByCategoryIdAsync(categoryId);
            return all.Where(b => b.IsActive).ToList();
        }

        public async Task<Blog?> GetBlogBySlugAsync(string slug)
        {
            return await _blogRepository.GetBlogBySlugAsync(slug);
        }


    }
}
