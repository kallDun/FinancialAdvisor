using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Repositories.Operations;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Operations
{
    [CustomService]
    public class LimitByCategoryService : ILimitByCategoryService
    {
        private readonly ITransactionGroupService _transactionGroupService;
        private readonly ILimitByCategoryRepository _repository;
        private readonly IBoundaryUnitsService _boundaryUnitsService;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;

        public LimitByCategoryService(ITransactionGroupService transactionGroupService, ILimitByCategoryRepository repository, 
            IBoundaryUnitsService boundaryUnitsService, IAccountService accountService, ICategoryService categoryService)
        {
            _transactionGroupService = transactionGroupService;
            _repository = repository;
            _boundaryUnitsService = boundaryUnitsService;
            _accountService = accountService;
            _categoryService = categoryService;
        }

        public async Task<LimitByCategory> Create(User user, string accountName, string categoryName, decimal limit, byte groupCount, DateTime groupDateFrom)
        {
            var expenseLimitMax = _boundaryUnitsService.GetMaxExpenseLimit(user.Id);
            if (limit < 0 || limit > expenseLimitMax) throw new ArgumentException($"Expense limit cannot be more than {expenseLimitMax} and less than 0");
            if (DateTime.Now < groupDateFrom) throw new ArgumentException("Start limit date cannot be in the future");
            
            int? accountId = string.IsNullOrEmpty(accountName) ? null : (await _accountService.GetByName(user.Id, accountName) 
                ?? throw new InvalidDataException($"Account with name {accountName} not found")).Id;
            int categoryId = (await _categoryService.GetByName(user.Id, categoryName))?.Id 
                ?? throw new InvalidDataException($"Category with name {categoryName} not found");

            if (await _repository.IsLimitExpenseUnique(categoryId, limit) == false) 
                throw new ArgumentException($"Limit with category {categoryName} and amount {limit} already exists");

            if (_boundaryUnitsService.GetMaxLimitsInOneCategory(user.Id) <= await _repository.Count(user.Id, categoryName))
                throw new ArgumentException($"You have reached the limit of 'limits' in one category");

            var (index, dateFrom, dateTo) = _transactionGroupService.CalculateGroupIndexForDateByUser(user, groupDateFrom);
            var limitByCategory = new LimitByCategory()
            {
                UserId = user.Id,
                AccountId = accountId,
                CategoryId = categoryId,
                ExpenseLimit = limit,
                GroupCount = groupCount,
                GroupIndexFrom = index,
                CreatedAt = DateTime.Now,
            };

            var added = await _repository.Add(limitByCategory);
            return await _repository.GetById(added.Id) 
                ?? throw new InvalidDataException("Limit by category was not created");
        }

        public async Task<IList<LimitByCategory>> GetManyLimitByCategories(User user, string categoryName, bool withData)
        {
            return await _repository.GetByCategoryWithInfo(user.Id, categoryName, withData);
        }

        public async Task<decimal> GetTotalExpenseAmountByLimit(User user, LimitByCategory limitByCategory, DateTime date)
        {
            var (index, dateFrom, dateTo) = _transactionGroupService.CalculateGroupIndexForDateByUser(user, date);
            return await _repository.GetTotalExpenseAmount(limitByCategory, index);
        }

        public async Task<LimitByCategory?> GetLimitByCategory(User user, string categoryName, decimal expense, bool withData)
        {
            return await _repository.GetByCategoryAndExpense(user.Id, categoryName, expense, withData);
        }

        public int GetDaysLeft(User user, LimitByCategory limitByCategory, DateTime date)
        {
            var (index, dateFrom, dateTo) = _transactionGroupService.CalculateGroupIndexForDateByUser(user, date);
            int indexStart = (index - limitByCategory.GroupIndexFrom) / limitByCategory.GroupCount;
            int groupsLeft = limitByCategory.GroupCount - ((index - limitByCategory.GroupIndexFrom) % limitByCategory.GroupCount);
            (dateFrom, dateTo) = _transactionGroupService.CalculateDateForIndexByUser(user, indexStart, groupsLeft);
            return (dateTo - date).Days - ((((int)date.DayOfWeek) - ((int)DayOfWeek.Monday) + 7) % 7) + 1;
        }

        public async Task<bool> HasAny(int userId, string categoryName)
        {
            return await _repository.HasAny(userId, categoryName);
        }

        public async Task<bool> IsTransactionExceedLimit(User user, string categoryName, decimal expensePositiveAmount, DateTime date)
        {
            var (index, dateFrom, dateTo) = _transactionGroupService.CalculateGroupIndexForDateByUser(user, date);

            IList<LimitByCategory> limits = await _repository.GetByCategoryWithInfo(user.Id, categoryName, true);

            foreach (var limit in limits)
            {
                var totalExpenseAmount = await _repository.GetTotalExpenseAmount(limit, index);
                return totalExpenseAmount + expensePositiveAmount > limit.ExpenseLimit;
            }
            return false;
        }
    }
}
