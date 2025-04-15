using BlogProject.Business.Abstract;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;

namespace BlogProject.Business.Concrete
{
    public class CategoryManager : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryManager(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Category>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Category> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task CreateAsync(Category category)
        {
            await _repository.AddAsync(category);
            await _repository.SaveAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _repository.Update(category);
            await _repository.SaveAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _repository.Delete(category);
            await _repository.SaveAsync();
        }
    }
}
