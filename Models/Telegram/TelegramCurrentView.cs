namespace FinancialAdvisorTelegramBot.Models.Telegram
{
    public class TelegramUserView
    {
        public int Id { get; set; }

        public int TelegramUserId { get; set; }

        public virtual TelegramUser? TelegramUser { get; set; }

        public string? CurrentCommandType { get; set; }

        public string? CurrentCommandData { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
