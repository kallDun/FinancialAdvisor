using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Models.Operations
{
    public class LimitByCategory
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public virtual User? User { get; set; }

        public int? AccountId { get; set; }

        public virtual Account? Account { get; set; }

        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public decimal Limit { get; set; }
        
        public int GroupCount { get; set; }

        public int GroupIndexFrom { get; set; }

        public bool Enabled { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
