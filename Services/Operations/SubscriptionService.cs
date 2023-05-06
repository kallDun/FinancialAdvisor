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
            if (await _repository.GetByName(userId, name, loadAllData: false) is not null) 
                throw new ArgumentException("Subscription with this name already exists", nameof(name));
            
            var minBoundaryAmount = _boundaryUnitsService.GetMinTransactionAmount(userId);
            var maxBoundaryAmount = _boundaryUnitsService.GetMaxTransactionAmount(userId);
            if (amount < minBoundaryAmount || amount > maxBoundaryAmount)
                throw new ArgumentException($"Subscription amount must be between {minBoundaryAmount} and {maxBoundaryAmount}");
            if (amount == 0) throw new ArgumentException("Subscription cannot be equal to zero", nameof(amount));

            if (_boundaryUnitsService.GetMaxCategoriesInOneUser(userId) <= await _repository.Count(userId))
                throw new ArgumentException("You have reached the limit of subscriptions");

            Subscription entity = new()
            {
                UserId = userId,
                AccountId = accountId,
                CategoryId = categoryId,
                Name = name,
                Amount = amount,
                PaymentDay = paymentDay,
                AutoPay = autoPay,
                NextPaymentDate = InitNextPaymentDate(paymentDay),
                CreatedAt = DateTime.Now
            };
            var added = await _repository.Add(entity);
            return await _repository.GetById(added.Id) 
                ?? throw new Exception("Subscription was not created");
        }

        private static DateTime InitNextPaymentDate(byte paymentDay)
        {
            DateTime now = DateTime.Now;
            DateTime thisMonthDate = new(now.Year, now.Month, paymentDay);            
            return thisMonthDate < now ? GetNextPaymentDate(thisMonthDate, paymentDay) : thisMonthDate;
        }

        private static DateTime GetNextPaymentDate(DateTime previousPaymentDay, byte paymentDay)
        {
            DateTime nextMonthDate = previousPaymentDay.AddMonths(1);
            return new(nextMonthDate.Year, nextMonthDate.Month, paymentDay);
        }

        public async Task<IList<Subscription>> GetAllWithData()
        {
            return await _repository.GetAllWithData();
        }

        public async Task<Subscription?> GetByName(int userId, string name, bool loadAllData = false)
        {
            return await _repository.GetByName(userId, name, loadAllData);
        }

        public async Task<bool> HasAny(int userId, string? accountName = null)
        {
            return accountName is null
                ? await _repository.HasAny(userId)
                : await _repository.HasAny(userId, accountName);
        }

        public async Task<IList<Subscription>> LoadAllWithDataByUser(int userId, string? accountName = null)
        {
            return accountName is null
                ? await _repository.LoadAllWithDataByUser(userId)
                : await _repository.LoadAllWithDataByUser(userId, accountName);
        }

        public async Task<Transaction?> CreateTransaction(Subscription subscription, Account account, DateTime transactionTime, SubscriptionTransactionType transactionType)
        {
            if (subscription.User is null) throw new ArgumentException("Subscription must have user");
            if (subscription.Amount == 0) throw new InvalidDataException("Subscription amount cannot be equal to zero");
            if (string.IsNullOrEmpty(subscription.Name)) throw new InvalidDataException("Subscription must have name");
            
            var dbTransaction = await _repository.DatabaseContext.Database.BeginTransactionAsync();
            Transaction? transaction = null;
            
            if (transactionType is not SubscriptionTransactionType.Delayed)
            {
                transaction = await _transactionService.CreateWithoutDatabaseTransaction(subscription.User,
                    subscription.Amount, subscription.Name, account.Id, subscription.CategoryId, transactionTime,
                    details: $"{transactionType} payed/received money for subscription {subscription.Name}", new List<Subscription> { subscription });
            }
            if (transactionType is SubscriptionTransactionType.Default)
            {
                subscription.NextPaymentDate = GetNextPaymentDate(subscription.NextPaymentDate, subscription.PaymentDay);
            }
            else if (transactionType is SubscriptionTransactionType.Late)
            {
                subscription.OverduePaymentNumber--;
            }
            else if (transactionType is SubscriptionTransactionType.Delayed)
            {
                subscription.OverduePaymentNumber++;
                subscription.NextPaymentDate = GetNextPaymentDate(subscription.NextPaymentDate, subscription.PaymentDay);
            }
            else throw new ArgumentException("Unknown transaction type");
            
            subscription.UpdatedAt = DateTime.Now;
            await _repository.Update(subscription);
            await dbTransaction.CommitAsync();
            return transaction;
        }
    }
}
