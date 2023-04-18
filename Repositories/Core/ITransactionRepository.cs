using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface ITransactionRepository : IReadonlyRepository<Transaction>
    {
        Task<bool> HasAnyTransactionByCategory(int categoryId);
    }
}
