using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Subscriptions;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;
using System.Text.RegularExpressions;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class AccountByNameMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Account menu";
        public static string DEFAULT_STYLE => "/account_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IAccountService _accountService;

        public AccountByNameMenuCommand(IBot bot, ITelegramUserService telegramUserService, IAccountService accountService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _accountService = accountService;
        }

        public bool IsContextMenu(string contextMenu) => Regex.IsMatch(contextMenu, $"^({ContextMenus.Accounts})[/](.*?)$");

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 1 && split[0] == ContextMenus.Accounts
                && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
                && user.UserId != null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (split.Length < 2) throw new InvalidDataException("Invalid context menu");
            string name = split[1];

            List<string> buttons = await _accountService.GetByName(user.UserId 
                ?? throw new InvalidDataException("User id cannot be null"), name) is not null
                ? new()
                {
                    ViewAccountCommand.TEXT_STYLE,
                    SubscriptionMenuCommand.TEXT_STYLE,
                    AccountsMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    AccountsMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Accounts}/{name}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Account {name} ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
