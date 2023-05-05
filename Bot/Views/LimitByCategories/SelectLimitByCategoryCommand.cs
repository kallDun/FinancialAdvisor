using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Services.Telegram;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.LimitByCategories
{
    public class SelectLimitByCategoryCommand : ICommand
    {
        public static string TEXT_STYLE => "Select limit";
        public static string DEFAULT_STYLE => "/select";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        [CommandPropertySerializable] public int Status { get; set; }

        private readonly IBot _bot;
        private readonly ITelegramUserService _telegramUserService;
        private readonly IUserService _userService;
        private readonly ILimitByCategoryService _limitByCategoryService;

        public SelectLimitByCategoryCommand(IBot bot, ITelegramUserService telegramUserService, IUserService userService, ILimitByCategoryService limitByCategoryService)
        {
            _bot = bot;
            _telegramUserService = telegramUserService;
            _userService = userService;
            _limitByCategoryService = limitByCategoryService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 3 && split[2] == ContextMenus.LimitByCategory
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE);
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string[] contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            if (Status == 0)
            {
                await WriteAvailableLimits(user, contextMenuSplit);
                Status++;
            }
            else
            {
                await SetLimitContextMenu(update, user, contextMenuSplit);
                IsFinished = true;
            }
        }

        private async Task SetLimitContextMenu(UpdateArgs update, TelegramUser user, string[] contextMenuSplit)
        {
            string categoryName = contextMenuSplit[1];
            decimal expense = Converters.ToDecimal(update.GetTextData());
            LimitByCategory? limit = await _limitByCategoryService.GetLimitByCategory(await _userService.GetById(user.UserId
                ?? throw new InvalidDataException("User id is null"))
                ?? throw new InvalidDataException("User not found"), categoryName, expense, false);
            if (limit is null) throw new ArgumentNullException($"Limit {expense} not found");

            await _telegramUserService.SetContextMenu(user, $"{ContextMenus.Category}/{categoryName}/{ContextMenus.LimitByCategory}/{expense}");
        }

        private async Task WriteAvailableLimits(TelegramUser user, string[] contextMenuSplit)
        {
            string categoryName = contextMenuSplit[1];
            IList<LimitByCategory> limits = await _limitByCategoryService.GetManyLimitByCategories(await _userService.GetById(user.UserId 
                ?? throw new InvalidDataException("User id is null"))
                ?? throw new InvalidDataException("User not found"), categoryName, false);
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"<b>↓ Available limits for category {categoryName} ↓</b>",
                Placeholder = "Select command",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = limits
                .Select(limit => new List<InlineButton>() { new InlineButton(
                        $"{limit.ExpenseLimit}", $"{limit.ExpenseLimit}") })
                .ToList()
            });
        }
    }
}
