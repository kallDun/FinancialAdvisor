using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Repositories.Operations;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    [CustomService]
    public class TargetService : ITargetService
    {
        private readonly ITargetRepository _repository;

        public TargetService(ITargetRepository repository)
        {
            _repository = repository;
        }

        public Task<TargetSubAccount> Create(Account account, string name, string? description, decimal goalAmount)
        {
            throw new NotImplementedException();
        }

        public Task<IList<TargetSubAccount>> GetAll(int userId, string accountName)
        {
            throw new NotImplementedException();
        }

        public Task<TargetSubAccount> GetByName(int userId, string accountName, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> HasAny(int userId, string accountName)
        {
            return await _repository.HasAny(userId, accountName);
        }
    }
}
