using Telegram.Bot;

namespace FinancialAdvisorTelegramBot.Bot
{
    public interface IBot
    {
        TelegramBotClient BotClient { get; }

        Task Write(string message, long chatId);
    }
}
