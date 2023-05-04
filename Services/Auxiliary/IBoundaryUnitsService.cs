using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Services.Auxiliary
{
    public interface IBoundaryUnitsService
    {
        decimal GetMaxExpenseLimit();

        decimal GetMaxTransactionAmount(int accountId);
        
        decimal GetMinTransactionAmount(int accountId);

        decimal GetMinSubscriptionAmount(int userId, int? accountId);
        
        decimal GetMaxSubscriptionAmount(int userId, int? accountId);

        int GetMaxTransactionGroupsAgo(User user);
    }
}
