using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly ITransactionGroupService _transactionGroupService;
        private readonly IBoundaryUnitsService _boundaryUnitsService;

        public TransactionService(ITransactionRepository repository, ITransactionGroupService transactionGroupService, IBoundaryUnitsService boundaryUnitsService)
        {
            _repository = repository;
            _transactionGroupService = transactionGroupService;
            _boundaryUnitsService = boundaryUnitsService;
        }

        public async Task<Transaction> Create(User user, decimal amount, string communicator, int accountId, 
            int categoryId, DateTime transactionTime, string? details, bool useDbTransaction = true)
        {
            Transaction transaction;
            if (useDbTransaction)
            {
                using var dbTransaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();
                transaction = await CreateMethodWithoutDbTransaction(user, amount, communicator, accountId, categoryId, transactionTime, details);
                await dbTransaction.CommitAsync();
            }
            else
            {
                transaction = await CreateMethodWithoutDbTransaction(user, amount, communicator, accountId, categoryId, transactionTime, details);
            }
            return transaction;
        }

        private async Task<Transaction> CreateMethodWithoutDbTransaction(User user, decimal amount, string communicator, 
            int accountId, int categoryId, DateTime transactionTime, string? details)
        {
            (int index, DateTime groupDateFrom, DateTime groupDateTo) = _transactionGroupService.CalculateGroupIndexForDateByUser(user, transactionTime);
            TransactionGroup group = await _transactionGroupService.GetOtherwiseCreate(accountId, index, groupDateFrom, groupDateTo);
            await _transactionGroupService.CreateTransactionGroupByCategoryIfNotExist(group.Id, categoryId);

            var transactionMax = _boundaryUnitsService.GetMaxTransactionAmount(accountId);
            var transactionMin = _boundaryUnitsService.GetMinTransactionAmount(accountId);
            if (amount > transactionMax || amount < transactionMin) 
                throw new ArgumentException($"Amount in transaction cannot be more than {transactionMax} and less than {transactionMin}");
            
            Transaction transaction = new()
            {
                Amount = amount,
                TransactionGroup = group,
                Communicator = communicator,
                AccountId = accountId,
                CategoryId = categoryId,
                TransactionTime = transactionTime,
                Details = details
            };
            Transaction created = await _repository.Add(transaction);
            Transaction result = await _repository.GetById(created.Id) ?? throw new InvalidDataException("Transaction was not created");
            return result;
        }
    }
}
