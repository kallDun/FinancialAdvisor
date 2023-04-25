using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    public interface ISubscriptionService
    {
        Task<bool> HasAny(int userId, string? accountName = null);

        Task<IList<Subscription>> LoadAllWithAccounts(int userId, string? accountName = null);

        Task<Subscription> Create(int userId, int? accountId, string name, decimal amount, byte paymentDay);

        DateTime GetNextPaymentDate(byte paymentDay, DateTime lastPaymentDay);
    }
}
