using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Repositories.Operations
{
    public interface ITargetRepository : IRepository<TargetSubAccount>
    {
        Task<bool> HasAny(string accountName);
    }
}
