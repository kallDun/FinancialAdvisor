using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class TransactionGroupService : ITransactionGroupService
    {
        private readonly ITransactionGroupRepository _repository;

        public TransactionGroupService(ITransactionGroupRepository repository)
        {
            _repository = repository;
        }

        public async Task<TransactionGroup> Create(int accountId, int index, DateTime dateFrom, DateTime dateTo)
        {
            if (dateTo < dateFrom) throw new ArgumentException("DateTo must be greater than DateFrom");
            if (index < 0) throw new ArgumentException("Index must be greater than 0");
                        
            TransactionGroup group = new()
            {
                AccountId = accountId,
                Index = index,
                DateFrom = dateFrom,
                DateTo = dateTo,
                CreatedAt = DateTime.Now
            };
            if (!(group.CreatedAt > dateFrom && group.CreatedAt < dateTo))
                throw new ArgumentException("Transaction group must be created at the same time as date range");

            var created = await _repository.Add(group);
            return await _repository.GetById(created.Id) ?? 
                throw new InvalidDataException("Transaction group was not created");
        }

        public async Task<TransactionGroup> GetOtherwiseCreate(int accountId, int index, DateTime dateFrom, DateTime dateTo)
        {
            if (!(DateTime.Now > dateFrom && DateTime.Now < dateTo))
                throw new ArgumentException("Cannot add transaction into previous or future group");

            return await _repository.GetByIndex(accountId, index)
                ?? await Create(accountId, index, dateFrom, dateTo);
        }

        public (int Index, DateTime DateFrom, DateTime DateTo) CalculateGroupIndexForDateByUser(User user, DateTime date)
        {
            var startDate = user.CreatedAt.Date;
            startDate = startDate.AddDays(((int)startDate.DayOfWeek * -1) + 1); // always make start day to monday

            var totalDays = (date.Date - startDate).Days;
            var index = totalDays / user.DaysInGroup;

            var dateFrom = startDate.AddDays(index * user.DaysInGroup);
            var dateTo = dateFrom.AddDays(user.DaysInGroup);

            return (index, dateFrom, dateTo);
        }
    }
}
