using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ITargetService
    {
        Task<TargetSubAccount> Create(Account account, string name, string? description, decimal goalAmount);

        Task<IList<TargetSubAccount>> GetAll(int userId, string accountName);

        Task<TargetSubAccount> GetByName(int userId, string accountName, string name);

        Task<bool> HasAny(int userId, string accountName);
    }
}
