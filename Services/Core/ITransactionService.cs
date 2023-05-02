using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ITransactionService
    {
        Task<Transaction> CreateWithoutDatabaseTransaction(User user, decimal amount, string communicator,
            int accountId, int categoryId, DateTime transactionTime, string? details, IList<Subscription>? subscriptions = null);

        Task<Transaction> Create(User user, decimal amount, string communicator,
            int accountId, int categoryId, DateTime transactionTime, string? details);
    }
}
