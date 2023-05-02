using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Models.Operations
{
    public class Subscription
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public virtual User? User { get; set; }

        public int? AccountId { get; set; }
        
        public virtual Account? Account { get; set; }

        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public string? Name { get; set; }

        public decimal Amount { get; set; }

        public byte PaymentDay { get; set; }

        public bool AutoPay { get; set; }

        public int OverduePaymentNumber { get; set; }

        public virtual IList<Transaction>? Transactions { get; set; }

        public DateTime? LastPaymentDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
