using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
    public class TransactionGroupService : ITransactionGroupService
    {
        private readonly ITransactionGroupRepository _repository;
        private readonly ITransactionGroupToCategoryRepository _groupByCategoryRepository;
        private readonly IBoundaryUnitsService _boundaryUnitsService;

        public TransactionGroupService(ITransactionGroupRepository repository, ITransactionGroupToCategoryRepository groupByCategoryRepository, IBoundaryUnitsService boundaryUnitsService)
        {
            _repository = repository;
            _groupByCategoryRepository = groupByCategoryRepository;
            _boundaryUnitsService = boundaryUnitsService;
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

            var created = await _repository.Add(group);
            return await _repository.GetById(created.Id) ?? 
                throw new InvalidDataException("Transaction group was not created");
        }

        public async Task<TransactionGroup> GetOtherwiseCreate(User user, int accountId, int index, DateTime dateFrom, DateTime dateTo)
        {
            int indexNow = CalculateGroupIndexForDateByUser(user, DateTime.Now).Index;
            if (index > indexNow) throw new ArgumentException("Cannot add transaction into future group");
            int maxGroupsAgo = _boundaryUnitsService.GetMaxTransactionGroupsAgo(user);
            if (index < indexNow - maxGroupsAgo) throw new ArgumentException($"Cannot add transaction into more than {maxGroupsAgo} groups ago");

            return await _repository.GetByIndex(accountId, index)
                ?? await Create(accountId, index, dateFrom, dateTo);
        }

        public async Task CreateTransactionGroupByCategoryIfNotExist(int transactionGroupId, int categoryId)
        {
            if (await _groupByCategoryRepository.Find(transactionGroupId, categoryId) == false)
            {
                await _groupByCategoryRepository.Add(new TransactionGroupToCategory()
                {
                    TransactionGroupId = transactionGroupId,
                    CategoryId = categoryId
                });
            }
        }

        public (int Index, DateTime DateFrom, DateTime DateTo) CalculateGroupIndexForDateByUser(User user, DateTime date)
        {
            var startDate = user.CreatedAt.Date;
            startDate = startDate.AddDays(-GetDayOfWeekIndex(startDate.DayOfWeek, DayOfWeek.Monday)); // always make start day to monday

            var totalDays = (date.Date - startDate).Days;
            var index = totalDays / user.DaysInGroup;
            if (index < 0) index--;

            var dateFrom = startDate.AddDays(index * user.DaysInGroup);
            var dateTo = dateFrom.AddDays(user.DaysInGroup);

            return (index, dateFrom, dateTo);
        }

        private int GetDayOfWeekIndex(DayOfWeek dayOfWeekNow, DayOfWeek startDayOfWeek)
        {
            return ((int)dayOfWeekNow - ((int)startDayOfWeek) + 7) % 7;
        }

        public (DateTime DateFrom, DateTime DateTo) CalculateDateForIndexByUser(User user, int index, int span = 1)
        {
            var (zeroIndex, dateFrom, dateTo) = CalculateGroupIndexForDateByUser(user, user.CreatedAt);
            dateFrom = dateFrom.AddDays(index * user.DaysInGroup);
            dateTo = dateFrom.AddDays(span * user.DaysInGroup);
            return (dateFrom, dateTo);
        }
    }
}
