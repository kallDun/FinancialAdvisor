using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Services.Auxiliary;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Units
{
    [CustomService]
    public class BoundaryUnitsService : IBoundaryUnitsService
    {
        public decimal GetMaxExpenseLimit()
        {
            return 100000000;
        }

        public decimal GetMaxTransactionAmount(int accountId)
        {
            return 100000;
        }

        public decimal GetMinTransactionAmount(int accountId)
        {
            return -100000;
        }

        public decimal GetMaxSubscriptionAmount(int userId, int? accountId)
        {
            return 100000;
        }

        public decimal GetMinSubscriptionAmount(int userId, int? accountId)
        {
            return -100000;
        }

        public int GetMaxTransactionGroupsAgo(User user)
        {
            return 4;
        }
    }
}
