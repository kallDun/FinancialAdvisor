using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ICategoryService
    {
        Task<IList<Category>> GetUserCategories(int userId);

        Task<Category> CreateCategory(int userId, string name, string? description);
        
        Task<Category> UpdateCategory(Category category);

        Task DeleteCategory(Category category);

        Task<Category> GetOrOtherwiseCreateCategory(int userId, string categoryName);
    }
}
