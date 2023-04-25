using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Repositories.Operations;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    [CustomService]
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;

        public SubscriptionService(ISubscriptionRepository repository)
        {
            _repository = repository;
        }

        
        public async Task<Subscription> Create(int userId, int? accountId, string name, decimal amount, byte paymentDay)
        {
            Subscription entity = new()
            {
                UserId = userId,
                AccountId = accountId,
                Name = name,
                Amount = amount,
                PaymentDay = paymentDay,
                LastPaymentDate = DateTime.Now,
                CreatedAt = DateTime.Now,
            };
            var added = await _repository.Add(entity);
            return await _repository.GetById(added.Id) 
                ?? throw new Exception("Subscription was not created");
        }

        public DateTime GetNextPaymentDate(byte paymentDay, DateTime lastPaymentDay)
        {
            var now = DateTime.Now;
            var days_in_current_month = DateTime.DaysInMonth(now.Year, now.Month);
            var paymentDateInCurrentMonth = new DateTime(now.Year, now.Month,
                days_in_current_month < paymentDay ? days_in_current_month : paymentDay);
            if (lastPaymentDay < paymentDateInCurrentMonth) return paymentDateInCurrentMonth;

            var year = now.Month == 12 ? now.Year + 1 : now.Year;
            var month = now.Month == 12 ? 1 : now.Month + 1;
            var days_in_next_month = DateTime.DaysInMonth(year, month);
            var paymentDateInNextMonth = new DateTime(year, month,
                days_in_next_month < paymentDay ? days_in_next_month : paymentDay);
            return paymentDateInNextMonth;
        }

        public async Task<bool> HasAny(int userId, string? accountName = null)
        {
            return accountName is null
                ? await _repository.HasAny(userId)
                : await _repository.HasAny(userId, accountName);
        }

        public async Task<IList<Subscription>> LoadAllWithAccounts(int userId, string? accountName = null)
        {
            return accountName is null
                ? await _repository.LoadAllWithAccounts(userId)
                : await _repository.LoadAllWithAccounts(userId, accountName);
        }
    }
}
