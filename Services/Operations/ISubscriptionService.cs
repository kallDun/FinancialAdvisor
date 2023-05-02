using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ISubscriptionService
    {
        Task<bool> HasAny(int userId, string? accountName = null);

        Task<Subscription?> GetByName(int userId, string name);

        Task<IList<Subscription>> LoadAllWithAccounts(int userId, string? accountName = null);

        Task<Subscription> Create(int userId, int? accountId, int categoryId, string name, decimal amount, byte paymentDay, bool autoPay);

        DateTime GetNextPaymentDate(Subscription subscription);

        Task<IList<Subscription>> GetAllWithData();

        Task<Transaction> CreateTransaction(Subscription subscription, DateTime transactionTime);
    }
}
