namespace FinancialAdvisorTelegramBot.Services.Auxiliary
{
    public interface IBoundaryUnitsService
    {
        public decimal GetMaxExpenseLimit();

        public decimal GetMaxTransactionAmount(int accountId);
        
        public decimal GetMinTransactionAmount(int accountId);

        public decimal GetMinSubscriptionAmount(int userId, int? accountId);
        
        public decimal GetMaxSubscriptionAmount(int userId, int? accountId);
    }
}
