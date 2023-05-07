using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Statistics.Model;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Statistics
{
    [CustomService]
    public class StatisticsService : IStatisticsService
    {
        private readonly ITransactionGroupRepository _repository;
        private readonly ITransactionGroupService _transactionGroupService;

        public StatisticsService(ITransactionGroupRepository repository, ITransactionGroupService transactionGroupService)
        {
            _repository = repository;
            _transactionGroupService = transactionGroupService;
        }

        public async Task<IList<GroupsStatistic>> GetGroupsStatistics(User user, byte groupsBundlesInOneStatistic, byte bundlesCount)
        {
            if (bundlesCount <= 0 || bundlesCount > 12) throw new ArgumentException("Cannot create more than 12 bundles and less than 1");
            if (groupsBundlesInOneStatistic <= 0 || groupsBundlesInOneStatistic > 20) throw new ArgumentException("Bundle cannot have more than 20 groups and less than 1");
            int indexTo = _transactionGroupService.CalculateGroupIndexForDateByUser(user, DateTime.Now).Index;
            int indexFrom = indexTo - (bundlesCount * groupsBundlesInOneStatistic) + 1;
            List<GroupsStatistic> result = new();

            IList<TransactionGroup> transactionGroups = await _repository.GetGroupsBetweenIndexesByUser(user.Id, indexFrom, indexTo);

            for (int index = indexFrom; index <= indexTo; index += groupsBundlesInOneStatistic)
            {
                DateTime dateFrom = _transactionGroupService.CalculateDateForIndexByUser(user, index).DateFrom;
                DateTime dateTo = _transactionGroupService.CalculateDateForIndexByUser(user, index + groupsBundlesInOneStatistic - 1).DateTo;
                
                List<TransactionGroup> transactionSubGroups = transactionGroups
                    .Where(group => group.Index >= index && group.Index < index + groupsBundlesInOneStatistic)
                    .ToList();

                if (transactionSubGroups.Any())
                {
                    IEnumerable<IGrouping<string, TransactionGroupToCategory>> categories = transactionSubGroups
                        .SelectMany(x => x?.TransactionGroupToCategories ?? new List<TransactionGroupToCategory>())
                        .GroupBy(x => x.Category?.Name ?? "-- No name --");

                    result.Add(new GroupsStatistic()
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        TotalExpense = transactionSubGroups.Sum(x => x.TotalExpense),
                        TotalExpensePerCategories = categories.ToDictionary(x => x.Key, x => x.Sum(x => x.TotalExpense)),
                        TotalIncome = transactionSubGroups.Sum(x => x.TotalIncome),
                        TotalIncomePerCategories = categories.ToDictionary(x => x.Key, x => x.Sum(x => x.TotalIncome))
                    });
                }
                else
                {
                    result.Add(new GroupsStatistic()
                    {
                        DateFrom = dateFrom,
                        DateTo = dateTo,
                        TotalExpense = 0,
                        TotalIncome = 0,
                    });
                }
            }
            return result;
        }
    }
}
