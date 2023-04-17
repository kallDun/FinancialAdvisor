namespace FinancialAdvisorTelegramBot.Models.Telegram
{
    public class TelegramCurrentCommand
    {
        public int Id { get; set; }

        public int TelegramUserId { get; set; }

        public virtual TelegramUser? TelegramUser { get; set; }

        public string? Type { get; set; }

        public string? DataJson { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
