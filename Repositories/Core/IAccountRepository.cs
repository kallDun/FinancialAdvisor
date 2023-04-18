using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<IList<Account>> GetAccountsByUserId(int userId);

        Task<Account?> GetAccountByName(int userId, string name);

        Task<bool> IsAccountNameUnique(int userId, string name);
    }
}
