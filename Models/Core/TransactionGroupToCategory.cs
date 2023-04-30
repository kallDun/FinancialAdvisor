namespace FinancialAdvisorTelegramBot.Models.Core
{
    public class TransactionGroupToCategory
    {
        public int TransactionGroupId { get; set; }

        public virtual TransactionGroup? TransactionGroup { get; set; }

        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }
        
        public decimal TotalIncome { get; set; }

        public decimal TotalExpense { get; set; }
    }
}
