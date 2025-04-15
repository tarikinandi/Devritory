using BlogProject.DataAccess.Abstract;
using BlogProject.DataAccess.Concrete.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlogProject.DataAccess.Concrete.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter) => await _dbSet.Where(filter).ToListAsync();
        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter) => await _dbSet.FirstOrDefaultAsync(filter);
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);
        public async Task SaveAsync() => await _context.SaveChangesAsync();
    }
}
