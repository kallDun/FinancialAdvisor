using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;

        public AccountService(IAccountRepository accountRepository, ITransactionService transactionService, ICategoryService categoryService)
        {
            _repository = accountRepository;
            _transactionService = transactionService;
            _categoryService = categoryService;
        }
        

        public async Task<Account> Create(User user, string name, string? description, decimal startBalance)
        {
            using var transaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();

            if (await GetByName(user.Id, name) is not null) throw new ArgumentException("Account with this name already exists");
            
            Account entity = new()
            {
                UserId = user.Id,
                Name = name,
                Description = description,
                CreatedAt = DateTime.Now
            };
            Account added = await _repository.Add(entity);
            Account account = await _repository.GetById(added.Id) ?? throw new Exception("Account was not created");
            
            if (startBalance != 0)
            {
                Category defaultCategory = await _categoryService.GetOrOtherwiseCreateCategory(user.Id, CategoryNames.Default);

                Transaction addedStartBalanceTransaction = await _transactionService.CreateWithoutDatabaseTransaction(user, startBalance,
                    "Start balance", added.Id, defaultCategory.Id, DateTime.Now, details: $"Start balance for account {name}");
            }
            
            await transaction.CommitAsync();
            return account;
        }

        public async Task<(Transaction From, Transaction To)> Transfer(User user, Account accountFrom, Account accountTo, 
            decimal amount, int categoryId, DateTime transactionTime, string? details)
        {
            if (accountFrom.UserId != user.Id || accountTo.UserId != user.Id) throw new ArgumentException("Accounts do not belong to one user");
            if (accountFrom.Id == accountTo.Id) throw new ArgumentException("Accounts must be different");
            if (amount <= 0) throw new ArgumentException("Amount must be positive");
            if (accountFrom.Name is null || accountTo.Name is null) throw new ArgumentException("Account name cannot be null");

            using var dbTransaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();  
            
            Transaction transactionFrom = await _transactionService.CreateWithoutDatabaseTransaction(user, amount * -1,
                    accountTo.Name, accountFrom.Id, categoryId, transactionTime, $"Transfer to {accountTo.Name}. {details}");
            Transaction transactionTo = await _transactionService.CreateWithoutDatabaseTransaction(user, amount,
                    accountFrom.Name, accountTo.Id, categoryId, transactionTime, $"Transfer from {accountFrom.Name}. {details}");
            
            await dbTransaction.CommitAsync();

            return (transactionFrom, transactionTo);
        }

        public async Task DeleteByName(int userId, string accountName)
        {
            Account? account = await _repository.GetAccountByName(userId, accountName);
            if (account is null) throw new ArgumentNullException($"Account with name {accountName} not found");
            await _repository.Delete(account);
        }

        public async Task<Account?> GetById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task<Account?> GetByName(int userId, string accountName)
        {
            return await _repository.GetAccountByName(userId, accountName);
        }

        public async Task<IList<Account>> GetByUser(int userId)
        {
            return await _repository.GetAccountsByUserId(userId);
        }

        public async Task<bool> HasAny(int userId)
        {
            return await _repository.HasAny(userId);
        }

        public async Task<Account> Update(Account account)
        {
            account.UpdatedAt = DateTime.Now;
            return await _repository.Update(account);
        }
    }
}
