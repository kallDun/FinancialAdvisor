using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ICategoryService
    {
        Task<bool> HasAny(int userId);
        
        Task<IList<Category>> GetAll(int userId);

        Task<Category?> GetByName(int userId, string name);

        Task<Category> CreateCategory(int userId, string name, string? description);

        Task DeleteByName(int userId, string name);

        Task<Category> GetOrOtherwiseCreateCategory(int userId, string categoryName);

        Task<Category> Update(int userId, Category category, bool nameUpdated);
    }
}
