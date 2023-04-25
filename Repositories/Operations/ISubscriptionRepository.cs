using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {   
        Task<bool> HasAny(int userId, string? accountName = null);
        
        Task<IList<Subscription>> LoadAllWithAccounts(int userId, string? accountName = null);
    }
}
