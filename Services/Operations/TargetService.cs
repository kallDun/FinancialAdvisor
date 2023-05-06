using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Repositories.Operations;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    [CustomService]
    public class TargetService : ITargetService
    {
        private readonly ITargetRepository _repository;
        private readonly ITransactionService _transactionService;
        private readonly IBoundaryUnitsService _boundaryUnitsService;

        public TargetService(ITargetRepository repository, ITransactionService transactionService, IBoundaryUnitsService boundaryUnitsService)
        {
            _repository = repository;
            _transactionService = transactionService;
            _boundaryUnitsService = boundaryUnitsService;
        }

        public async Task<TargetSubAccount> Create(Account account, string name, string? description, decimal goalAmount)
        {
            if (await _repository.IsUnique(account.Id, name) == false) 
                throw new ArgumentException($"Target with name {name} already exists");

            if (_boundaryUnitsService.GetMaxTargetsInOneAccount(account.UserId) <= await _repository.Count(account.Id))
                throw new ArgumentException("You have reached the limit of targets in one account");

            var target = new TargetSubAccount
            {
                AccountId = account.Id,
                Name = name,
                Description = description,
                GoalAmount = goalAmount
            };
            var created = await _repository.Add(target);
            return await _repository.GetById(created.Id) 
                ?? throw new InvalidDataException("Target was not created");
        }

        public async Task<Transaction> CreateTransaction(User user, string targetName, decimal positiveAmountForTarget, int accountId, int categoryId, DateTime transactionTime)
        {
            TargetSubAccount target = await _repository.GetByName(accountId, targetName) 
                ?? throw new ArgumentException($"Target with name {targetName} not found");

            var dbTransaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();

            Transaction transaction = await _transactionService.CreateWithoutDatabaseTransaction(user, positiveAmountForTarget * -1, targetName, accountId, categoryId, 
                transactionTime, details: $"Payed/received money for target {targetName}", targetSubAccounts: new List<TargetSubAccount> { target });

            target.CurrentBalance += positiveAmountForTarget;
            target.UpdatedAt = DateTime.Now;
            await _repository.Update(target);

            await dbTransaction.CommitAsync();
            return transaction;
        }

        public async Task<IList<TargetSubAccount>> GetAll(int userId, string accountName)
        {
            return await _repository.GetAll(userId, accountName);
        }

        public async Task<TargetSubAccount?> GetByName(int userId, string accountName, string targetName)
        {
            return await _repository.GetByName(userId, accountName, targetName);
        }

        public async Task<bool> HasAny(int userId, string accountName)
        {
            return await _repository.HasAny(userId, accountName);
        }
    }
}
