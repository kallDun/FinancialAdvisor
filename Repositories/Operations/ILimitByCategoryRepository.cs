using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    public interface ILimitByCategoryRepository : IRepository<LimitByCategory>
    {
        Task<bool> IsLimitExpenseUnique(int categoryId, decimal limit);
    }
}
