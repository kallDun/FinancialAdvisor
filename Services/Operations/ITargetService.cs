using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ITargetService
    {
        Task<TargetSubAccount> Create(Account account, string name, string? description, decimal goalAmount);

        Task<Transaction> CreateTransaction(User user, string targetName, decimal positiveAmountForTarget, int accountId, int categoryId, DateTime transactionTime);

        Task<IList<TargetSubAccount>> GetAll(int userId, string accountName);

        Task<TargetSubAccount?> GetByName(int userId, string accountName, string targetNam);

        Task<bool> HasAny(int userId, string accountName);
    }
}
