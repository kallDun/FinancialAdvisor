using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;

namespace FinancialAdvisorTelegramBot.Bot.Views.Targets
{
    public class TargetByNameMenuCommand : ICommand
    {
        public static string TEXT_STYLE => "Target menu";
        public static string DEFAULT_STYLE => "/target_menu";

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ITargetService _targetService;

        public TargetByNameMenuCommand(IBot bot, ITelegramUserService telegramUserService, ITargetService targetService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _targetService = targetService;
        }

        public bool IsContextMenu(string[] contextMenu)
            => contextMenu.Length == 4 && contextMenu[0] == ContextMenus.Account && contextMenu[2] == ContextMenus.Target;

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length >= 3 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var split = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string accountName = split[1];
            string targetName = split[3];

            List<string> buttons = await _targetService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id cannot be null"), accountName, targetName) is not null
                ? new()
                {
                    ViewTargetCommand.TEXT_STYLE,
                    CreateTargetTransactionCommand.TEXT_STYLE,
                    DeleteTargetCommand.TEXT_STYLE,
                    TargetsMenuCommand.TEXT_STYLE
                }
                : new()
                {
                    TargetsMenuCommand.TEXT_STYLE
                };

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Account}/{accountName}/{ContextMenus.Target}/{targetName}");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Target <code>{targetName}</code> ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.ReplyKeyboard,
                ReplyKeyboardButtons = buttons,
            });
        }
    }
}
