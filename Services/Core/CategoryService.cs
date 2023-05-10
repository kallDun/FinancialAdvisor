using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBoundaryUnitsService _boundaryUnitsService;

        public CategoryService(ICategoryRepository repository, ITransactionRepository transactionRepository, IBoundaryUnitsService boundaryUnitsService)
        {
            _repository = repository;
            _transactionRepository = transactionRepository;
            _boundaryUnitsService = boundaryUnitsService;
        }

        public async Task<Category> CreateCategory(int userId, string name, string? description)
        {
            if (!(await _repository.IsCategoryNameUnique(userId, name))) 
                throw new Exception("Category name must be unique");

            if (_boundaryUnitsService.GetMaxCategoriesInOneUser(userId) <= await _repository.Count(userId))
                throw new ArgumentException("You have reached the limit of categories");

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

        public async Task DeleteByName(int userId, string name)
        {
            Category category = await GetByName(userId, name) 
                ?? throw new ArgumentException("Category not found");
            await _repository.Delete(category);
        }

        public async Task<Category> Update(int userId, Category category, bool nameUpdated)
        {
            category.UpdatedAt = DateTime.Now;
            if (nameUpdated)
            {
                if (!(await _repository.IsCategoryNameUnique(userId, category.Name
                    ?? throw new InvalidDataException("Category name cannot be null"))))
                    throw new Exception("Category name must be unique");
            }
            return await _repository.Update(category);
        }
    }
}
