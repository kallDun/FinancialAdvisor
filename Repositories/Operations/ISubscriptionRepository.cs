using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<int> Count(int userId);

        Task<IList<Subscription>> GetAllWithData();

        Task<Subscription?> GetByName(int userId, string name, bool loadAllData);

        Task<bool> HasAny(int userId, string? accountName = null);
        
        Task<IList<Subscription>> LoadAllWithDataByUser(int userId, string? accountName = null);
    }
}
