using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public interface ITransactionService
    {
        Task<Transaction> Create(decimal amount, string communicator, 
            int accountId, int categoryId, DateTime transactionTime, string? details);
    }
}
