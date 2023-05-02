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

        public async Task<bool> HasAny(string accountName)
        {
            return await _repository.HasAny(accountName);
        }
    }
}
