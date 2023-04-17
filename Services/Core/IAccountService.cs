using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface IAccountService
    {
        Task<Account?> GetById(int accountId);

        Task<Account> Create(User user, string name, string? description, decimal startBalance);

        Task<Account> Update(Account account);

        Task DeleteById(int accountId);
    }
}
