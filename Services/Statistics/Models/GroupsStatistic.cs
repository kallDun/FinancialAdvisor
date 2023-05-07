namespace FinancialAdvisorTelegramBot.Services.Statistics.Model
{
    public class GroupsStatistic
    {
        public decimal TotalExpense { get; set; }

        public Dictionary<string, decimal> TotalExpensePerCategories { get; set; } = new();

        public decimal TotalIncome { get; set; }

        public Dictionary<string, decimal> TotalIncomePerCategories { get; set; } = new();

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}
