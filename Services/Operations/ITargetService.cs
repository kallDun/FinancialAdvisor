using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ITargetService
    {
        Task<TargetSubAccount> Create(Account account, string name, string? description, decimal goalAmount);

        Task<bool> HasAny(string accountName);
    }
}
