using FinancialAdvisorTelegramBot.Models.Operations;

namespace FinancialAdvisorTelegramBot.Models.Core
{
    public class Transaction
    {
        public int Id { get; set; }

        public int TransactionGroupId { get; set; }

        public virtual TransactionGroup? TransactionGroup { get; set; }

        public int AccountId { get; set; }

        public virtual Account? Account { get; set; }

        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public decimal Amount { get; set; }
        
        public string? Communicator { get; set; }

        public DateTime TransactionTime { get; set; }

        public string? Details { get; set; }

        public virtual IList<Subscription>? Subscriptions { get; set; }

        public virtual IList<TargetSubAccount>? TargetSubAccounts { get; set; }
    }
}
