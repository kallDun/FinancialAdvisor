using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Models.Operations
{
    public class TargetAccount
    {
        public int Id { get; set; }

        public decimal GoalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual Account? Account { get; set; }
    }
}
