using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface IAccountService
    {
        Task<Account?> GetById(int id);

        Task<IList<Account>> GetByUser(int userId);

        Task<bool> HasAny(int userId);

        Task<Account?> GetByName(int userId, string accountName);

        Task<Account> Create(User user, string name, string? description, decimal startBalance, decimal creditLimit);

        Task<Account> Update(User profile, Account account, bool nameUpdated);

        Task DeleteByName(int userId, string accountName);

        Task<(Transaction From, Transaction To)> Transfer(User user, Account accountFrom, Account accountTo,
            decimal amount, int categoryId, DateTime transactionTime, string? details);
    }
}
