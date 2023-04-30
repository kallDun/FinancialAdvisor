using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ILimitByCategoryService
    {
        Task<LimitByCategory> Create(User user, string accountName, string categoryName, decimal limit, byte groupCount, DateTime groupDateFrom);
    }
}
