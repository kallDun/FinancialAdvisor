using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Services.Statistics.Model;

namespace FinancialAdvisorTelegramBot.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<IList<GroupsStatistic>> GetGroupsStatistics(User user, byte groupsBundlesInOneStatistic, byte bundlesCount);
    }
}
