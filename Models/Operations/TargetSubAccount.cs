using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Models.Operations
{
    public class TargetSubAccount
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public virtual Account? Account { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal GoalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
