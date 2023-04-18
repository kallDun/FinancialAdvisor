using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Core.Enumerations;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ITransactionService
    {
        Task<Transaction> Create(decimal amount, TransactionType type, string communicator, 
            int accountId, int categoryId, DateTime transactionTime, string? details);
    }
}
