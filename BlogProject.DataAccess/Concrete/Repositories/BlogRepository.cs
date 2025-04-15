using BlogProject.DataAccess.Abstract;
using BlogProject.DataAccess.Concrete.Context;
using BlogProject.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlogProject.DataAccess.Concrete.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Blog> _dbSet;

        public BlogRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Blog>();
        }

        public async Task AddAsync(Blog entity) => await _dbSet.AddAsync(entity);
        public void Delete(Blog entity) {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
        public async Task<List<Blog>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<List<Blog>> GetAllAsync(Expression<Func<Blog, bool>> filter) => await _dbSet.Where(filter).ToListAsync();
        public async Task<Blog?> GetAsync(Expression<Func<Blog, bool>> filter) => await _dbSet.FirstOrDefaultAsync(filter);
        public async Task<Blog?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public void Update(Blog entity) => _dbSet.Update(entity);
        public async Task SaveAsync() => await _context.SaveChangesAsync();

        public async Task<Blog?> GetBlogWithUserAndCategoriesAsync(int id)
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Categories)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Blog>> GetAllWithCategoriesAsync()
        {
            return await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Categories)
                .Include(b => b.Comments)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Blogs
                .Where(b => b.Categories.Any(c => c.Id == categoryId))
                .Include(b => b.User)
                .Include(b => b.Categories)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetActiveBlogsAsync()
        {
            return await _context.Blogs
                .Where(b => b.IsActive)
                .Include(b => b.User)
                .Include(b => b.Categories)
                .ToListAsync();
        }

        public async Task<List<Blog>> GetBlogsByUserIdAsync(int userId)
        {
            return await _context.Blogs
                .Where(b => b.UserId == userId)
                .Include(b => b.Categories)
                .OrderByDescending(b => b.PublishDate)
                .ToListAsync();
        }

        public async Task<Blog?> GetBlogBySlugAsync(string slug)
        {
            return await _context.Blogs
                .Include(b => b.Categories)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Slug == slug);
        }

    }
}
