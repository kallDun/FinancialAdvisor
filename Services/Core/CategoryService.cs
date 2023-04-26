using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ITransactionRepository _transactionRepository;


        public CategoryService(ICategoryRepository repository, ITransactionRepository transactionRepository)
        {
            _repository = repository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Category> CreateCategory(int userId, string name, string? description)
        {
            if (!(await _repository.IsCategoryNameUnique(userId, name))) 
                throw new Exception("Category name is not unique");
            
            Category category = new()
            {
                Name = name,
                Description = description,
                UserId = userId,
                CreatedAt = DateTime.Now,
            };
            Category added = await _repository.Add(category);
            return await _repository.GetById(added.Id) 
                ?? throw new Exception("Category was not created");
        }

        public async Task DeleteCategory(Category category)
        {
            if (await _transactionRepository.HasAnyTransactionByCategory(category.Id))
                throw new Exception("Cannot delete category with transactions");
            await _repository.Delete(category);
        }

        public async Task<Category> UpdateCategory(Category category)
        {
            category.UpdatedAt = DateTime.Now;
            return await _repository.Update(category);
        }

        public async Task<IList<Category>> GetAll(int userId)
        {
            return await _repository.GetCategoriesByUser(userId);
        }

        public async Task<Category> GetOrOtherwiseCreateCategory(int userId, string categoryName)
        {
            return await _repository.GetCategoryByName(userId, categoryName) 
                ?? await CreateCategory(userId, categoryName, null);
        }

        public async Task<bool> HasAny(int userId)
        {
            return await _repository.HasAny(userId);
        }

        public async Task<Category?> GetByName(int userId, string name)
        {
            return await _repository.GetCategoryByName(userId, name);
        }
    }
}
