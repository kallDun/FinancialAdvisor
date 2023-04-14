using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Utils;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinancialAdvisorTelegramBot.Bot
{
    public class Bot : IBot
    {
        public TelegramBotClient BotClient { get; private set; }

        public Bot(IOptions<BotSettings> options)
        {
            BotClient = new TelegramBotClient(options.Value.Token 
                ?? throw new ArgumentNullException(nameof(options.Value.Token)));
        }

        public async Task Write(TelegramUser user, TextMessageArgs messageArgs)
        {
            try
            {
                List<string> messages = new();
                MessagesExtensions.SplitMessage(messageArgs.Text, ref messages);

                foreach (string message in messages)
                {
                    await BotClient.SendTextMessageAsync(user.ChatId, message,
                        parseMode: messageArgs.ParseMode,
                        replyMarkup: messageArgs.HideKeyboard ? new ReplyKeyboardRemove() : null,
                        disableWebPagePreview: messageArgs.DisableWebPagePreview);
                }
                //Logger.Log($"Bot replied to user {user.Username} with: {messageArgs.Text}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured in 'Write' botMessage method: " + e.Message);
                //Logger.Log($"Exception occured in 'Write' bot method in user '{user?.Username}': " + e.Message);
            }
        }

        public async Task SendInlineKeyboard(TelegramUser user, InlineKeyboardArgs keyboardArgs)
        {
            try
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(keyboardArgs.Buttons.Select(x => new KeyboardButton[] { new KeyboardButton(x) }))
                {
                    ResizeKeyboard = keyboardArgs.ResizeKeyboard,
                    InputFieldPlaceholder = keyboardArgs.Placeholder,
                    OneTimeKeyboard = keyboardArgs.OneTimeKeyboard
                };
                await BotClient.SendTextMessageAsync(user.ChatId, keyboardArgs.Text,
                    replyMarkup: replyKeyboardMarkup,
                    parseMode: ParseMode.Html);
                //Logger.Log($"Bot created inline keyboard for user '{user.Username}' with next buttons: {string.Join(", ", keyboardArgs.Buttons)}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured in 'SendInlineKeyboard' botMessage method: " + e.Message);
                //Logger.Log($"Exception occured in 'SendInlineKeyboard' bot method in user '{user?.Username}': " + e.Message);
            }
        }
    }
}
