namespace FinancialAdvisorTelegramBot.Models.Core
{
    public class TransactionGroup
    {
        public int Id { get; set; }

        public int? AccountId { get; set; }

        public virtual Account? Account { get; set; }

        public int Index { get; set; }

        public decimal TotalIncome { get; set; }

        public decimal TotalExpense { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual IList<Transaction>? Transactions { get; set; }

        public virtual IList<TransactionGroupToCategory>? TransactionGroupToCategories { get; set; }
    }
}
