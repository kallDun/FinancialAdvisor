using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Telegram.Bot;

namespace FinancialAdvisorTelegramBot.Bot
{
    public interface IBot
    {
        TelegramBotClient BotClient { get; }

        Task Write(TelegramUser user, TextMessageArgs messageArgs);

        Task SendInlineKeyboard(TelegramUser user, InlineKeyboardArgs keyboardArgs);
    }
}
