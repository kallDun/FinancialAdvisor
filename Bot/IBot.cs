using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot;

namespace FinancialAdvisorTelegramBot.Bot
{
    public interface IBot
    {
        TelegramBotClient BotClient { get; }

        Task WriteByChatId(long chatId, TextMessageArgs messageArgs);

        Task Write(TelegramUser user, TextMessageArgs messageArgs);
    }
}
