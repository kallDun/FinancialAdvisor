using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface IAccountService
    {
        Task<IList<Account>> GetByUser(int userId);

        Task<bool> HasAny(int userId);

        Task<Account?> GetByName(int userId, string accountName);

        Task<Account> Create(User user, string name, string? description, decimal startBalance);

        Task<Account> Update(Account account);

        Task DeleteByName(int userId, string accountName);
    }
}
