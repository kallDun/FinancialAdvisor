using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    public interface ITargetRepository : IRepository<TargetSubAccount>
    {
        Task<IList<TargetSubAccount>> GetAll(int userId, string accountName);
        
        Task<TargetSubAccount?> GetByName(int userId, string accountName, string targetName);

        Task<TargetSubAccount?> GetByName(int accountId, string targetName);
        
        Task<bool> HasAny(int userId, string accountName);

        Task<bool> IsUnique(int accountId, string name);
    }
}
