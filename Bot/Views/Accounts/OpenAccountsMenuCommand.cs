using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Accounts
{
    public class OpenAccountsMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Accounts menu";
        public static string DEFAULT_STYLE => "/accounts_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IAccountService _accountService;

        public OpenAccountsMenuCommand(IBot bot, ITelegramUserService telegramUserService, IAccountService accountService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _accountService = accountService;
        }

        public bool IsContextMenu(TelegramUser user) => user.ContextMenu == ContextMenus.Accounts;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => (update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            List<string> buttons = await _accountService.HasAny(user.UserId 
                ?? throw new ArgumentNullException("User id cannot be null"))
                ? new()
                {
                    CreateAccountCommand.TEXT_STYLE
                }
                : new()
                {
                    CreateAccountCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, ContextMenus.Accounts);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Accounts menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
