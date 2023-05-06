namespace FinancialAdvisorTelegramBot.Services.Auxiliary
{
    public interface IBoundaryUnitsService
    {
        decimal GetMaxExpenseLimit(int userId);

        decimal GetMaxTransactionAmount(int userId);
        
        decimal GetMinTransactionAmount(int userId);

        int GetMaxTransactionGroupsAgo(int userId);

        int GetMaxAccountsInOneUser(int userId);

        int GetMaxCategoriesInOneUser(int userId);

        int GetMaxLimitsInOneCategory(int userId);

        int GetMaxTargetsInOneAccount(int userId);
    }
}
