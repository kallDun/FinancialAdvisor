using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Accounts;
using FinancialAdvisorTelegramBot.Bot.Views.Profiles;
using FinancialAdvisorTelegramBot.Bot.Views.Subscriptions;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class MainMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Main menu";
        public static string DEFAULT_STYLE => GeneralCommands.Help;

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IAccountService _accountService;

        public MainMenuCommand(IBot bot, ITelegramUserService telegramUserService, IAccountService accountService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _accountService = accountService;
        }

        public bool IsContextMenu(string[] contextMenu) 
            => contextMenu.Length == 1
            && contextMenu[0] == ContextMenus.MainMenu;

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            List<string> buttons = user.UserId != null
                ? new List<string>()
                {
                    ProfileMenuCommand.TEXT_STYLE,
                    AccountsMenuCommand.TEXT_STYLE
                }
                : new List<string>()
                {
                    ProfileMenuCommand.TEXT_STYLE
                };

            if (user.UserId != null && await _accountService.HasAny(user.UserId.Value))
            {
                buttons.AddRange(new List<string>()
                {
                    SubscriptionsMenuCommand.TEXT_STYLE
                });
            }

            await _telegramUserService.SetContextMenu(user, ContextMenus.MainMenu);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Main menu ↓</b>",
                Placeholder = "Select menu",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
