using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    public interface ILimitByCategoryRepository : IRepository<LimitByCategory>
    {
        Task<bool> HasAny(int userId, string categoryName);

        Task<IList<LimitByCategory>> GetByCategoryWithInfo(int userId, string categoryName, bool withData);
        
        Task<LimitByCategory?> GetByCategoryAndExpense(int userId, string categoryName, decimal expense, bool withData);
        
        Task<decimal> GetTotalExpenseAmount(LimitByCategory limitByCategory, int currentGroupIndex);

        Task<bool> IsLimitExpenseUnique(int categoryId, decimal limit);
    }
}
