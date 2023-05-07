using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Repositories.Core
{
    public interface ITransactionGroupRepository : IReadonlyRepository<TransactionGroup>
    {
        Task<TransactionGroup?> GetByIndex(int accountId, int index);

        Task<IList<TransactionGroup>> GetGroupsBetweenIndexesByUser(int userId, int indexFrom, int indexTo);
    }
}
