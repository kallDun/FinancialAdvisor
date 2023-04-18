using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Core.Enumerations;
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

            Account entity = new()
            {
                User = user,
                Name = name,
                Description = description
            };
            Account added = await _repository.Add(entity);

            Category defaultCategory = await _categoryService.GetOtherwiseCreateDefaultCategory(user.Id);

            Transaction addedStartBalanceTransaction = await _transactionService.Create(
                startBalance, TransactionType.Income, "Start balance", 
                added.Id, defaultCategory.Id, DateTime.Now, null);
            
            Account account = await _repository.GetById(added.Id) ?? throw new Exception("Account was not created");
            if (account.CurrentBalance != startBalance) throw new Exception("Start balance transaction was not created");

            await transaction.CommitAsync();
            return account;
        }

        public async Task DeleteByName(User user, string accountName)
        {
            Account? account = await _repository.GetAccountByName(user.Id, accountName);
            if (account is null) throw new ArgumentNullException($"Account with name {accountName} not found");
            await _repository.Delete(account);
        }

        public async Task<Account?> GetByName(User user, string accountName)
        {
            return await _repository.GetAccountByName(user.Id, accountName);
        }

        public async Task<IList<Account>> GetByUser(User user)
        {
            return await _repository.GetAccountsByUserId(user.Id);
        }

        public async Task<Account> Update(Account account)
        {
            return await _repository.Update(account);
        }
    }
}
