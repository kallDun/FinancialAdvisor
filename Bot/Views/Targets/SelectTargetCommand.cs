using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Targets
{
    public class SelectTargetCommand : ICommand
    {
        public static string TEXT_STYLE => "Select target";
        public static string DEFAULT_STYLE => "/select";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly ITargetService _targetService;

        public SelectTargetCommand(IBot bot, ITelegramUserService telegramUserService, ITargetService targetService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _targetService = targetService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 3 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (Status == 0)
            {
                await WriteAvailableTargets(user, contextMenuSplit);
                Status++;
            }
            else
            {
                await SetTargetContextMenu(update, user, contextMenuSplit);
                IsFinished = true;
            }
        }

        private async Task SetTargetContextMenu(UpdateArgs update, TelegramUser user, string[] contextMenuSplit)
        {
            string name = update.GetTextData().Trim();
            TargetSubAccount target = await _targetService.GetByName(user.UserId
                ?? throw new InvalidDataException("User id is null"), contextMenuSplit[1], name);
            if (target is null) throw new ArgumentNullException("Category not found");

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Account}/{contextMenuSplit[1]}/{ContextMenus.Target}/{name}");
        }

        private async Task WriteAvailableTargets(TelegramUser user, string[] contextMenuSplit)
        {
            IList<TargetSubAccount> targets = await _targetService.GetAll(user.UserId
                ?? throw new InvalidDataException("User id is null"), contextMenuSplit[1]);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = "<b>↓ Available targets ↓</b>",
                Placeholder = "Select target",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = targets
                .Select(target => new List<InlineButton>() { new InlineButton(
                        target.Name ?? "--Empty name--",
                        target.Name ?? GeneralCommands.Cancel) })
                .ToList()
            });
        }
    }
}
