using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Utils;
using Microsoft.Extensions.Options;
using Telegram.Bot;
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
            await WriteByChatId(user.ChatId, messageArgs);
        }

        public async Task WriteByChatId(long chatId, TextMessageArgs messageArgs)
        {
            try
            {
                List<string> messages = new();
                MessagesExtensions.SplitMessage(messageArgs.Text, ref messages);

                IReplyMarkup? replyMarkup = GetReplyMarkupFromMessageArgs(messageArgs);

                for (int i = 0; i < messages.Count; i++)
                {
                    if (i != messages.Count - 1)
                    {
                        await BotClient.SendTextMessageAsync(chatId, messages[i],
                            parseMode: messageArgs.ParseMode,
                            disableWebPagePreview: messageArgs.DisableWebPagePreview);
                    }
                    else
                    {
                        await BotClient.SendTextMessageAsync(chatId, messages[i],
                            parseMode: messageArgs.ParseMode,
                            disableWebPagePreview: messageArgs.DisableWebPagePreview,
                            replyMarkup: replyMarkup);
                    }
                }
                //Logger.Log($"Bot replied to user {user.Username} with: {messageArgs.Text}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured in 'Write' botMessage method: " + e.Message);
                //Logger.Log($"Exception occured in 'Write' bot method in user '{user?.Username}': " + e.Message);
            }
        }

        private IReplyMarkup? GetReplyMarkupFromMessageArgs(TextMessageArgs messageArgs)
        {
            if (messageArgs.MarkupType == ReplyMarkupType.InlineKeyboard)
            {
                InlineKeyboardMarkup inlineKeyboardButton = new(messageArgs.InlineKeyboardButtons
                    .Select(list => list.Select(item => InlineKeyboardButton.WithCallbackData(item.Text, item.CallbackData))));
                return inlineKeyboardButton;
            }
            else if (messageArgs.MarkupType == ReplyMarkupType.ReplyKeyboard)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(messageArgs.ReplyKeyboardButtons
                    .Select(x => new KeyboardButton[] { new KeyboardButton(x) }))
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = messageArgs.Placeholder
                };
                return replyKeyboardMarkup;
            }
            else if (messageArgs.MarkupType == ReplyMarkupType.KeyboardRemove)
            {
                return new ReplyKeyboardRemove();
            }
            else if (messageArgs.MarkupType == ReplyMarkupType.ForceReply)
            {
                return new ForceReplyMarkup() { InputFieldPlaceholder = messageArgs.Placeholder };
            }
            else
            {
                return null;
            }
        }
    }
}
