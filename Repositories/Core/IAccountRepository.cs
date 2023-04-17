using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<IList<Account>> GetAccountsByUserId(int userId);
    }
}
