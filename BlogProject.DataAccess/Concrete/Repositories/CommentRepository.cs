using BlogProject.DataAccess.Abstract;
using BlogProject.DataAccess.Concrete.Context;
using BlogProject.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.DataAccess.Concrete.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment entity)
        {
            await _context.Comments.AddAsync(entity);
            await _context.SaveChangesAsync();  
        }

        public async Task UpdateAsync(Comment entity)
        {
            _context.Comments.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetCommentsByBlogIdAsync(int blogId)
        {
            return await _context.Comments
                .Include(c => c.User) 
                .Where(c => c.BlogId == blogId)
                .OrderByDescending(c => c.CommentDate)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetAllWithBlogAndUserAsync()
        {
            return await _context.Comments
                .Include(c => c.Blog)
                .Include(c => c.User)
                .Where(c => c.Blog != null && c.User != null)
                .ToListAsync();
        }
        public async Task DeleteAsync(Comment comment)
        {
            var childComments = await _context.Comments
                .Where(c => c.ParentCommentId == comment.Id)
                .ToListAsync();

            _context.Comments.RemoveRange(childComments);

            _context.Comments.Remove(comment);

            await SaveAsync();
        }

    }
}
