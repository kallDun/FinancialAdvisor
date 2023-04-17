using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface IAccountService
    {
        Task<IList<Account>> GetByUser(User user);

        Task<Account?> GetByName(User user, string accountName);

        Task<Account> Create(User user, string name, string? description, decimal startBalance);

        Task<Account> Update(Account account);

        Task DeleteByName(User user, string accountName);
    }
}
