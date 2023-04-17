using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        public AccountService(IAccountRepository accountRepository)
        {
            _repository = accountRepository;
        }
        

        public async Task<Account> Create(User user, string name, string? description, decimal startBalance)
        {
            Account entity = new()
            {
                User = user,
                Name = name,
                Description = description
            };
            int id = await _repository.Add(entity);
            Account account = await _repository.GetById(id) ?? throw new Exception("Account not found");
            // add first transaction
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
