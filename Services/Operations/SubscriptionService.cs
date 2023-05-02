using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Repositories.Operations;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    [CustomService]
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;
        private readonly IBoundaryUnitsService _boundaryUnitsService;
        private readonly ITransactionService _transactionService;

        public SubscriptionService(ISubscriptionRepository repository, IBoundaryUnitsService boundaryUnitsService, ITransactionService transactionService)
        {
            _repository = repository;
            _boundaryUnitsService = boundaryUnitsService;
            _transactionService = transactionService;
        }

        public async Task<Subscription> Create(int userId, int? accountId, int categoryId, string name, decimal amount, byte paymentDay, bool autoPay)
        {
            if (paymentDay < 1 || paymentDay > 31) 
                throw new ArgumentException("Payment day must be between 1 and 31", nameof(paymentDay));
            if (await _repository.GetByName(userId, name) is not null) 
                throw new ArgumentException("Subscription with this name already exists", nameof(name));
            
            var minBoundaryAmount = _boundaryUnitsService.GetMinSubscriptionAmount(userId, accountId);
            var maxBoundaryAmount = _boundaryUnitsService.GetMaxSubscriptionAmount(userId, accountId);
            if (amount < minBoundaryAmount || amount > maxBoundaryAmount)
                throw new ArgumentException($"Subscription amount must be between {minBoundaryAmount} and {maxBoundaryAmount}");
            if (amount == 0) throw new ArgumentException("Subscription cannot be equal to zero", nameof(amount));

            Subscription entity = new()
            {
                UserId = userId,
                AccountId = accountId,
                CategoryId = categoryId,
                Name = name,
                Amount = amount,
                PaymentDay = paymentDay,
                AutoPay = autoPay,
                LastPaymentDate = DateTime.Now.Date,
                CreatedAt = DateTime.Now
            };
            var added = await _repository.Add(entity);
            return await _repository.GetById(added.Id) 
                ?? throw new Exception("Subscription was not created");
        }

        public async Task<IList<Subscription>> GetAllWithData()
        {
            return await _repository.GetAllWithData();
        }

        public async Task<Subscription?> GetByName(int userId, string name)
        {
            return await _repository.GetByName(userId, name);
        }

        public DateTime GetNextPaymentDate(Subscription subscription)
        {
            byte paymentDay = subscription.PaymentDay;
            DateTime lastPaymentDay = subscription.LastPaymentDate?.Date ?? DateTime.Now;

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

        public async Task<Transaction> CreateTransaction(Subscription subscription, DateTime transactionTime)
        {
            if (subscription.AccountId is null) throw new ArgumentException("Subscription must have account");
            if (subscription.User is null) throw new ArgumentException("Subscription must have user");
            if (subscription.Amount == 0) throw new InvalidDataException("Subscription amount cannot be equal to zero");
            if (string.IsNullOrEmpty(subscription.Name)) throw new InvalidDataException("Subscription must have name");
            
            var dbTransaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();

            Transaction transaction = await _transactionService.CreateWithoutDatabaseTransaction(subscription.User, 
                subscription.Amount, subscription.Name, subscription.AccountId.Value, subscription.CategoryId, transactionTime, 
                details: $"Payed/received money for subscription {subscription.Name}", new List<Subscription> { subscription });

            subscription.LastPaymentDate = transactionTime;
            subscription.UpdatedAt = DateTime.Now;
            await _repository.Update(subscription);

            await dbTransaction.CommitAsync();
            return transaction;
        }
    }
}
