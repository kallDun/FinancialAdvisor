using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Units
{
    [CustomService]
    public class BoundaryUnitsService : IBoundaryUnitsService
    {
        public decimal GetMaxExpenseLimit(int userId) => 100000000;

        public decimal GetMaxTransactionAmount(int userId) => 1000000;

        public decimal GetMinTransactionAmount(int userId) => -1000000;

        public int GetMaxTransactionGroupsAgo(int userId) => 4;

        public int GetMaxAccountsInOneUser(int userId) => 20;

        public int GetMaxCategoriesInOneUser(int userId) => 20;

        public int GetMaxLimitsInOneCategory(int userId) => 10;

        public int GetMaxTargetsInOneAccount(int userId) => 10;
    }
}
