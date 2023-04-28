using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface ITransactionGroupToCategoryRepository : IReadonlyRepository<TransactionGroupToCategory>
    {
        Task<bool> Find(int transactionGroupId, int categoryId);
    }
}
