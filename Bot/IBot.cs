using Telegram.Bot;

namespace FinancialAdvisorTelegramBot.Bot
{
    public interface IBot
    {
        public TelegramBotClient BotClient { get; }
    }
}
