using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;

namespace FinancialAdvisorTelegramBot.Models.Core
{
    public class User
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual TelegramUser? TelegramUser { get; set; }

        public virtual IList<Account>? Accounts { get; set; }

        public virtual IList<Category>? Categories { get; set; }

        public virtual IList<Subscription>? Subscriptions { get; set; }
    }
}
