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
            var expenseLimitMax = _boundaryUnitsService.GetMaxExpenseLimit();
            if (limit < 0 || limit > expenseLimitMax) throw new ArgumentException($"Expense limit icannot be more than {expenseLimitMax} and less than 0");
            if (DateTime.Now < groupDateFrom) throw new ArgumentException("Start limit date cannot be in the future");

            int? accountId = accountName is null ? null : (await _accountService.GetByName(user.Id, accountName) 
                ?? throw new InvalidDataException($"Account with name {accountName} not found")).Id;
            int categoryId = (await _categoryService.GetByName(user.Id, categoryName))?.Id 
                ?? throw new InvalidDataException($"Category with name {categoryName} not found");

            if (await _repository.IsLimitExpenseUnique(categoryId, limit) == false) 
                throw new ArgumentException($"Limit with category {categoryName} and amount {limit} already exists");

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
    }
}
