using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ITransactionGroupService
    {
        Task<TransactionGroup> Create(int accountId, int index, DateTime dateFrom, DateTime dateTo);

        Task<TransactionGroup> GetOtherwiseCreate(int accountId, int index, DateTime dateFrom, DateTime dateTo);

        (int Index, DateTime DateFrom, DateTime DateTo) CalculateGroupIndexForDateByUser(User user, DateTime date);
    }
}
