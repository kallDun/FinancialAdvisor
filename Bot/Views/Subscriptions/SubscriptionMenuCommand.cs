using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Accounts;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Telegram;
using System.Text.RegularExpressions;

namespace FinancialAdvisorTelegramBot.Bot.Views.Subscriptions
{
    public class SubscriptionMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Subscription menu";
        public static string DEFAULT_STYLE => "/subscription_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;

        public SubscriptionMenuCommand(IBot bot, ITelegramUserService telegramUserService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
        }

        public bool IsContextMenu(string contextMenu) => Regex.IsMatch(contextMenu, $"^({ContextMenus.Accounts})[/](.*?)[/]({ContextMenus.Subscription})$");

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 2 && split[0] == ContextMenus.Accounts
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (split.Length < 2) throw new InvalidDataException("Invalid context menu");

            List<string> buttons = new()
            {
                AccountByNameMenuCommand.TEXT_STYLE
            };

            await _telegramUserService.SetContextMenu(user, $"{split[0]}/{split[1]}/{ContextMenus.Subscription}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Subscription menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
