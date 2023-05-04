using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ITransactionGroupService
    {
        Task<TransactionGroup> Create(int accountId, int index, DateTime dateFrom, DateTime dateTo);

        Task CreateTransactionGroupByCategoryIfNotExist(int transactionGroupId, int categoryId);

        Task<TransactionGroup> GetOtherwiseCreate(User user, int accountId, int index, DateTime dateFrom, DateTime dateTo);

        (int Index, DateTime DateFrom, DateTime DateTo) CalculateGroupIndexForDateByUser(User user, DateTime date);
        
        (DateTime DateFrom, DateTime DateTo) CalculateDateForIndexByUser(User user, int index, int span = 1);
    }
}
