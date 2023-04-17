using FinancialAdvisorTelegramBot.Models.Core;

namespace FinancialAdvisorTelegramBot.Models.Telegram
{
    public class TelegramUser
    {
        public int Id { get; set; }
        
        public long ChatId { get; set; }

        public long TelegramId { get; set; }

        public int? UserId { get; set; }

        public virtual User? User { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Username { get; set; }

        public string? LanguageCode { get; set; }

        public string? ContextMenu { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual TelegramCurrentCommand? CurrentCommand { get; set; }
    }
}
