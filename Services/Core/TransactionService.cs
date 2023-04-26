using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;

        public TransactionService(ITransactionRepository repository)
        {
            _repository = repository;
        }


        public async Task<Transaction> Create(decimal amount, string communicator, int accountId, int categoryId, DateTime transactionTime, string? details)
        {
            if (Math.Abs(amount) > 100000) throw new Exception("Amount in transaction cannot be more than 100000");
            Transaction transaction = new()
            {
                Amount = amount,
                Communicator = communicator,
                AccountId = accountId,
                CategoryId = categoryId,
                TransactionTime = transactionTime,
                Details = details
            };
            Transaction created = await _repository.Add(transaction);
            return await _repository.GetById(created.Id) ?? throw new Exception("Transaction was not created");
        }
    }
}
