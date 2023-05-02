using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views.Accounts;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Target
{
    public class TargetsMenu : ICommand
    {
        public static string TEXT_STYLE => "Targets";
        public static string DEFAULT_STYLE => "/targets_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ITargetService _targetService;

        public TargetsMenu(IBot bot, ITelegramUserService telegramUserService, ITargetService targetService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _targetService = targetService;
        }

        public bool IsContextMenu(string[] contextMenu) 
            => contextMenu.Length == 3 && contextMenu[0] == ContextMenus.Account && contextMenu[2] == ContextMenus.Target;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 2 && split[0] == ContextMenus.Account
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE)
                && user.UserId is not null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            string[] split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string accountName = split[1];

            List<string> buttons = await _targetService.HasAny(accountName)
                ? new()
                {
                    CreateTargetCommand.TEXT_STYLE
                }
                : new()
                {
                    CreateTargetCommand.TEXT_STYLE
                };

            // add back to previous menu button
            buttons.Add(AccountByNameMenuCommand.TEXT_STYLE);

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Account}/{accountName}/{ContextMenus.Target}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Target subaccounts menu ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
