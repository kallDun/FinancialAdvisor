using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ISubscriptionService
    {
        Task<bool> HasAny(int userId, string? accountName = null);
        
        Task<Subscription?> GetByName(int userId, string name, bool loadAllData = false);

        Task<IList<Subscription>> LoadAllWithDataByUser(int userId, string? accountName = null);

        Task<Subscription> Create(int userId, int? accountId, int categoryId, string name, decimal amount, byte paymentDay, bool autoPay);

        Task<IList<Subscription>> GetAllWithData();

        Task<Transaction?> CreateTransaction(Subscription subscription, Account account, DateTime transactionTime, SubscriptionTransactionType transactionType);

        Task DeleteByName(int userId, string subscriptionName);
    }
}
