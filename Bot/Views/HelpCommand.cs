using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Accounts;
using FinancialAdvisorTelegramBot.Bot.Views.Profiles;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views
{
    public class HelpCommand : ICommand
    {
        public static string TEXT_STYLE => "Main menu";
        public static string DEFAULT_STYLE => GeneralCommands.Help;

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;

        public HelpCommand(IBot bot, ITelegramUserService telegramUserService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
        }

        public bool IsContextMenu(TelegramUser user) => user.ContextMenu == ContextMenus.MainMenu;

        public bool CanExecute(UpdateArgs update, TelegramUser user) 
            => update.GetTextData() == DEFAULT_STYLE
            || update.GetTextData() == TEXT_STYLE;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            List<string> buttons = user.UserId != null
                ? new List<string>()
                {
                    OpenProfileMenuCommand.TEXT_STYLE,
                    OpenAccountsMenuCommand.TEXT_STYLE
                }
                : new List<string>()
                {
                    OpenProfileMenuCommand.TEXT_STYLE
                };

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
