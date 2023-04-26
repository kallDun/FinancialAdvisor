using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryByName(int userId, string name);

        Task<IList<Category>> GetCategoriesByUser(int userId);

        Task<bool> IsCategoryNameUnique(int userId, string name);
        
        Task<bool> HasAny(int userId);
    }
}
