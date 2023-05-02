using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ILimitByCategoryService
    {
        Task<LimitByCategory> Create(User user, string accountName, string categoryName, decimal limit, byte groupCount, DateTime groupDateFrom);

        Task<IList<LimitByCategory>> GetLimitByCategoriesInfo(User user, string categoryName);

        Task<decimal> GetTotalExpenseAmountByLimit(User user, LimitByCategory limitByCategory, DateTime date);

        Task<bool> IsTransactionExceedLimit(User user, string categoryName, decimal expensePositiveAmount, DateTime date);

        Task<bool> HasAny(int userId, string categoryName);

        int GetDaysLeft(User user, LimitByCategory limitByCategory, DateTime date);
    }
}
