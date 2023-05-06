using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
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
        private readonly IAccountRepository _accountRepository;

        public TransactionService(ITransactionRepository repository, ITransactionGroupService transactionGroupService, 
            IBoundaryUnitsService boundaryUnitsService, IAccountRepository accountRepository)
        {
            _repository = repository;
            _transactionGroupService = transactionGroupService;
            _boundaryUnitsService = boundaryUnitsService;
            _accountRepository = accountRepository;
        }

        public async Task<Transaction> Create(User user, decimal amount, string communicator, int accountId, int categoryId, DateTime transactionTime, string? details)
        {
            Transaction transaction;
            using var dbTransaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();
            transaction = await CreateWithoutDatabaseTransaction(user, amount, communicator, accountId, categoryId, transactionTime, details);
            await dbTransaction.CommitAsync();
            return transaction;
        }

        public async Task<Transaction> CreateWithoutDatabaseTransaction(User user, decimal amount, string communicator, int accountId, int categoryId, 
            DateTime transactionTime, string? details, IList<Subscription>? subscriptions = null, IList<TargetSubAccount>? targetSubAccounts = null)
        {
            (int index, DateTime groupDateFrom, DateTime groupDateTo) = _transactionGroupService.CalculateGroupIndexForDateByUser(user, transactionTime);
            TransactionGroup group = await _transactionGroupService.GetOtherwiseCreate(user, accountId, index, groupDateFrom, groupDateTo);
            await _transactionGroupService.CreateTransactionGroupByCategoryIfNotExist(group.Id, categoryId);

            var transactionMax = _boundaryUnitsService.GetMaxTransactionAmount(user.Id);
            var transactionMin = _boundaryUnitsService.GetMinTransactionAmount(user.Id);
            if (amount > transactionMax || amount < transactionMin)
                throw new ArgumentException($"Amount in transaction cannot be more than {transactionMax} and less than {transactionMin}");

            Account account = await _accountRepository.GetById(accountId)
                    ?? throw new ArgumentException("Account with this id does not exist");
            if (account.UserId != user.Id) throw new ArgumentException("Account does not belong to this user");

            if (amount < 0)
            {
                if (account.CurrentBalance + amount < account.CreditLimit * -1)
                    throw new ArgumentException("Transaction exceeds the credit limit");
            }

            Transaction transaction = new()
            {
                Amount = amount,
                TransactionGroup = group,
                Communicator = communicator,
                AccountId = accountId,
                CategoryId = categoryId,
                TransactionTime = transactionTime,
                Details = details,
                Subscriptions = subscriptions,
                TargetSubAccounts = targetSubAccounts
            };
            Transaction created = await _repository.Add(transaction);
            Transaction result = await _repository.GetById(created.Id) ?? throw new InvalidDataException("Transaction was not created");
            return result;
        }
    }
}
