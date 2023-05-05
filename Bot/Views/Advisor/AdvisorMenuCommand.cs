using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Advisor
{
    public class AdvisorMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Advisor";
        public static string DEFAULT_STYLE => "/advisor";
        
        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IAccountService _accountService;

        public AdvisorMenuCommand(IBot bot, ITelegramUserService telegramUserService, IAccountService accountService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _accountService = accountService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 1
            && contextMenu[0] == ContextMenus.Advisor;

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
                    GetSimpleAdviceMenuCommand.TEXT_STYLE,
                    MainMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    GetSimpleAdviceMenuCommand.TEXT_STYLE,
                    MainMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, ContextMenus.Advisor);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Advisor menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
